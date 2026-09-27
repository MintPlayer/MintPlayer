// S3 — identity round-trip without real credentials.
//   seed  <sqlConnectionString> <credentials.json>   insert 2 synthetic legacy users into a *_S3_Source copy
//   probe <baseUrl> <credentials.json>               log in through the running MintPlayer.Web and prove
//                                                    password / TOTP / recovery code / Administrator rights
// Only synthetic credentials are created or printed. Never point this at a real database.
using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

return args switch
{
    ["seed", var cs, var file] => await Seed(cs, file),
    ["probe", var url, var file] => await Probe(url, file),
    _ => Usage(),
};

static int Usage()
{
    Console.Error.WriteLine("usage: S3Probe seed <sqlConnectionString> <credentials.json> | probe <baseUrl> <credentials.json>");
    return 64;
}

static async Task<int> Seed(string connectionString, string file)
{
    var builder = new SqlConnectionStringBuilder(connectionString);
    if (!builder.InitialCatalog.EndsWith("_S3_Source", StringComparison.Ordinal))
        throw new InvalidOperationException("Refusing: seed only writes to a throwaway *_S3_Source copy.");

    var password = new SyntheticUser("s3.password", "s3-password@example.invalid", "S3-Passw0rd!only", Guid.NewGuid(), null, [], false);
    var codes = Enumerable.Range(0, 10).Select(_ => NewRecoveryCode()).ToList();
    var totp = new SyntheticUser("s3.totp.admin", "s3-totp-admin@example.invalid", "S3-Passw0rd!totp", Guid.NewGuid(), Base32.Encode(RandomNumberGenerator.GetBytes(20)), codes, true);

    // (a) the pre-2021 Identity V3 format (HMAC-SHA256, 10 000 iterations) — 183 production hashes;
    // (b) today's PasswordHasher (HMAC-SHA512, 100 000 iterations) — 539 production hashes.
    var hashA = LegacyV3Hash(password.Password, iterations: 10_000);
    var hashB = new PasswordHasher<object>().HashPassword(new object(), totp.Password);

    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    var adminRole = (Guid)(await new SqlCommand("SELECT Id FROM mintplay.AspNetRoles WHERE Name = 'Administrator'", connection).ExecuteScalarAsync())!;

    foreach (var (user, hash) in new[] { (password, hashA), (totp, hashB) })
    {
        var insert = new SqlCommand("""
            INSERT INTO mintplay.AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash,
                SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled,
                AccessFailedCount, PictureUrl, Bypass2faForExternalLogin)
            VALUES (@id, @name, UPPER(@name), @email, UPPER(@email), 1, @hash, @stamp, @cstamp, NULL, 0, @tfa, NULL, 1, 0, '', 0)
            """, connection);
        insert.Parameters.AddWithValue("@id", user.Id);
        insert.Parameters.AddWithValue("@name", user.UserName);
        insert.Parameters.AddWithValue("@email", user.Email);
        insert.Parameters.AddWithValue("@hash", hash);
        insert.Parameters.AddWithValue("@stamp", Base32.Encode(RandomNumberGenerator.GetBytes(20)));
        insert.Parameters.AddWithValue("@cstamp", Guid.NewGuid().ToString());
        insert.Parameters.AddWithValue("@tfa", user.TwoFactor);
        await insert.ExecuteNonQueryAsync();
    }
    foreach (var (name, value) in new[] { ("AuthenticatorKey", totp.AuthenticatorKey!), ("RecoveryCodes", string.Join(';', codes)) })
    {
        var token = new SqlCommand("INSERT INTO mintplay.AspNetUserTokens (UserId, LoginProvider, Name, Value) VALUES (@u, '[AspNetUserStore]', @n, @v)", connection);
        token.Parameters.AddWithValue("@u", totp.Id);
        token.Parameters.AddWithValue("@n", name);
        token.Parameters.AddWithValue("@v", value);
        await token.ExecuteNonQueryAsync();
    }
    var role = new SqlCommand("INSERT INTO mintplay.AspNetUserRoles (UserId, RoleId) VALUES (@u, @r)", connection);
    role.Parameters.AddWithValue("@u", totp.Id);
    role.Parameters.AddWithValue("@r", adminRole);
    await role.ExecuteNonQueryAsync();

    await File.WriteAllTextAsync(file, JsonSerializer.Serialize(new[] { password, totp }, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"seeded (a) {password.Id} password-only [{hashA[..12]}…], (b) {totp.Id} password+TOTP+{codes.Count} recovery codes+Administrator [{hashB[..12]}…]");
    Console.WriteLine($"synthetic credentials → {file}");
    return 0;
}

static async Task<int> Probe(string baseUrl, string file)
{
    var users = JsonSerializer.Deserialize<SyntheticUser[]>(await File.ReadAllTextAsync(file))!;
    var (a, b) = (users[0], users[1]);
    var failures = 0;
    void Check(string name, bool ok, string detail)
    {
        Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}  — {detail}");
        if (!ok) failures++;
    }

    // (a) password-only
    {
        using var c = new Client(baseUrl);
        var byEmail = await c.Login(a.Email, a.Password);
        // FINDING, not a pass/fail of the migration: legacy logs in by email (AccountRepository FindByEmailAsync);
        // MapIdentityApi's /login passes the 'email' field to PasswordSignInAsync(userName) = FindByNameAsync,
        // and 750/752 production users have UserName != Email.
        Console.WriteLine($"{(byEmail.IsSuccessStatusCode ? "NOTE" : "FINDING")}  (a) login with legacy email + password — {(int)byEmail.StatusCode} " +
            "(Identity API /login resolves the 'email' field with FindByNameAsync)");
        var byName = await c.Login(a.UserName, a.Password);
        Check("(a) login with legacy user name + password", byName.IsSuccessStatusCode, $"{(int)byName.StatusCode}");
        var me = await c.Me();
        Check("(a) /spark/auth/me is the migrated user", me.Contains(a.Id.ToString("D"), StringComparison.OrdinalIgnoreCase) || me.Contains(a.UserName), Short(me));
        var create = await c.CreateMediumType("S3 probe (should be denied)");
        Check("(a) non-admin cannot create a MediumType", create.StatusCode == HttpStatusCode.Forbidden, $"{(int)create.StatusCode}");
    }

    // (b) password + TOTP
    {
        using var c = new Client(baseUrl);
        var noCode = await c.Login(b.UserName, b.Password);
        var body = await noCode.Content.ReadAsStringAsync();
        Check("(b) password alone is challenged for 2FA", noCode.StatusCode == HttpStatusCode.Unauthorized && body.Contains("RequiresTwoFactor"), $"{(int)noCode.StatusCode} {Short(body)}");
        var code = Totp.Compute(b.AuthenticatorKey!, DateTimeOffset.UtcNow);
        var withCode = await c.Login(b.UserName, b.Password, twoFactorCode: code);
        Check("(b) password + current TOTP from the legacy authenticator key", withCode.IsSuccessStatusCode, $"{(int)withCode.StatusCode} (code {code})");
        var me = await c.Me();
        Check("(b) /spark/auth/me carries the Administrator role", me.Contains("Administrator"), Short(me));
        var create = await c.CreateMediumType("S3 probe medium type");
        var created = await create.Content.ReadAsStringAsync();
        Check("(b) Administrator creates a MediumType via /spark/po (with XSRF)", create.StatusCode == HttpStatusCode.Created, $"{(int)create.StatusCode} {Short(created)}");
        var wrongCode = await new Client(baseUrl).Login(b.UserName, b.Password, twoFactorCode: "000000");
        Check("(b) a wrong TOTP is rejected", !wrongCode.IsSuccessStatusCode, $"{(int)wrongCode.StatusCode}");
    }

    // (b) recovery code
    {
        var recovery = b.RecoveryCodes[3];
        using var c1 = new Client(baseUrl);
        var first = await c1.Login(b.UserName, b.Password, recoveryCode: recovery);
        Check("(b) a legacy recovery code redeems", first.IsSuccessStatusCode, $"{(int)first.StatusCode} (code {recovery})");
        using var c2 = new Client(baseUrl);
        var again = await c2.Login(b.UserName, b.Password, recoveryCode: recovery);
        Check("(b) the same recovery code cannot be reused", !again.IsSuccessStatusCode, $"{(int)again.StatusCode}");
        using var c3 = new Client(baseUrl);
        var other = await c3.Login(b.UserName, b.Password, recoveryCode: b.RecoveryCodes[7]);
        Check("(b) another legacy recovery code still redeems", other.IsSuccessStatusCode, $"{(int)other.StatusCode}");
    }

    // S6 through the app: the karaoke read model serves the migrated all-lines layout
    foreach (var songId in new[] { "Songs/23", "Songs/43", "Songs/129", "Songs/238", "Songs/291" })
    {
        using var c = new Client(baseUrl);
        var json = JsonNode.Parse(await c.Get($"/api/song/lyrics?id={Uri.EscapeDataString(songId)}"))!;
        var lines = json["text"]!.GetValue<string>().Split('\n').Length;
        var starts = json["timings"]![0]!["startTimes"]!.AsArray();
        var synced = starts.Count(x => x is not null);
        Check($"S6 {songId} /api/song/lyrics timing parallels all lines", starts.Count == lines, $"{starts.Count} start times for {lines} lines, {synced} synced, medium {json["timings"]![0]!["mediumUrl"]}");
    }

    Console.WriteLine(failures == 0 ? "S3 PROBE: ALL PASS" : $"S3 PROBE: {failures} FAILED");
    return failures == 0 ? 0 : 1;
}

static string Short(string s) => s.Length <= 160 ? s : s[..160] + "…";

static string NewRecoveryCode()
{
    // Identity's format: two groups of 5 chars from its base32-like alphabet, e.g. "3RXDQ-KW8M2"
    const string alphabet = "23456789BCDFGHJKMNPQRTVWXY";
    var chars = Enumerable.Range(0, 10).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray();
    return new string(chars, 0, 5) + "-" + new string(chars, 5, 5);
}

/// <summary>Identity PasswordHasher V3 layout with HMAC-SHA256 — what ASP.NET Core Identity ≤ 6 produced.</summary>
static string LegacyV3Hash(string password, int iterations)
{
    var salt = RandomNumberGenerator.GetBytes(16);
    var subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
    var output = new byte[13 + salt.Length + subkey.Length];
    output[0] = 0x01;
    BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(1), 1); // KeyDerivationPrf.HMACSHA256
    BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(5), (uint)iterations);
    BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(9), (uint)salt.Length);
    salt.CopyTo(output, 13);
    subkey.CopyTo(output, 13 + salt.Length);
    return Convert.ToBase64String(output);
}

record SyntheticUser(string UserName, string Email, string Password, Guid Id, string? AuthenticatorKey, List<string> RecoveryCodes, bool TwoFactor);

sealed class Client : IDisposable
{
    private const string MediumTypeObjectTypeId = "2f167ae5-6428-4552-a4c2-8e36a169c998"; // App_Data/Model/MediumType.json
    private readonly CookieContainer cookies = new();
    private readonly HttpClient http;
    private readonly Uri baseUri;

    public Client(string baseUrl)
    {
        baseUri = new Uri(baseUrl);
        http = new HttpClient(new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = false }) { BaseAddress = baseUri };
    }

    public Task<HttpResponseMessage> Login(string userNameOrEmail, string password, string? twoFactorCode = null, string? recoveryCode = null)
        => http.PostAsJsonAsync("/spark/auth/login?useCookies=true", new Dictionary<string, string?>
        {
            ["email"] = userNameOrEmail,
            ["password"] = password,
            ["twoFactorCode"] = twoFactorCode,
            ["twoFactorRecoveryCode"] = recoveryCode,
        });

    public Task<string> Get(string path) => http.GetStringAsync(path);

    public async Task<string> Me()
    {
        var response = await http.GetAsync("/spark/auth/me");
        return $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}";
    }

    public async Task<HttpResponseMessage> CreateMediumType(string name)
    {
        await http.GetAsync("/spark/auth/me"); // refresh the XSRF-TOKEN cookie for the signed-in identity
        var xsrf = cookies.GetCookies(baseUri)["XSRF-TOKEN"]?.Value;
        var request = new HttpRequestMessage(HttpMethod.Post, $"/spark/po/{MediumTypeObjectTypeId}")
        {
            Content = JsonContent.Create(new JsonObject
            {
                ["persistentObject"] = new JsonObject
                {
                    ["name"] = "MediumType",
                    ["objectTypeId"] = MediumTypeObjectTypeId,
                    ["attributes"] = new JsonArray(
                        new JsonObject { ["name"] = "Name", ["value"] = name, ["isValueChanged"] = true },
                        new JsonObject { ["name"] = "Visible", ["value"] = true, ["dataType"] = "boolean", ["isValueChanged"] = true }),
                },
            }),
        };
        if (xsrf != null) request.Headers.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(xsrf));
        return await http.SendAsync(request);
    }

    public void Dispose() => http.Dispose();
}

static class Base32
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string Encode(byte[] data)
    {
        var result = new System.Text.StringBuilder();
        int buffer = 0, bits = 0;
        foreach (var value in data)
        {
            buffer = (buffer << 8) | value;
            bits += 8;
            while (bits >= 5) { result.Append(Alphabet[(buffer >> (bits - 5)) & 31]); bits -= 5; }
            buffer &= (1 << bits) - 1;
        }
        if (bits > 0) result.Append(Alphabet[(buffer << (5 - bits)) & 31]);
        return result.ToString();
    }

    public static byte[] Decode(string input)
    {
        input = input.TrimEnd('=').ToUpperInvariant();
        var output = new List<byte>();
        int buffer = 0, bits = 0;
        foreach (var ch in input)
        {
            buffer = (buffer << 5) | Alphabet.IndexOf(ch);
            bits += 5;
            if (bits >= 8) { output.Add((byte)(buffer >> (bits - 8))); bits -= 8; }
            buffer &= (1 << bits) - 1;
        }
        return [.. output];
    }
}

static class Totp
{
    /// <summary>RFC 6238 (HMAC-SHA1, 30 s, 6 digits) — what Identity's AuthenticatorTokenProvider validates.</summary>
    public static string Compute(string base32Key, DateTimeOffset now)
    {
        var step = (ulong)(now.ToUnixTimeSeconds() / 30);
        Span<byte> counter = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(counter, step);
        var hash = HMACSHA1.HashData(Base32.Decode(base32Key), counter);
        var offset = hash[^1] & 0x0f;
        var binary = ((hash[offset] & 0x7f) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6");
    }
}
