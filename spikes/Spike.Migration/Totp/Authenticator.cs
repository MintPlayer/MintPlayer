using System.Security.Cryptography;

namespace Spike.Migration.Totp;

/// <summary>
/// Independent RFC 6238 TOTP generator that mimics what a user's authenticator app (Google
/// Authenticator, Authy, …) produces from the shared secret. It is deliberately NOT Spark/Identity
/// code: the 2FA-continuity test generates a code here and validates it through ASP.NET Core
/// Identity's real AuthenticatorTokenProvider. If Identity accepts a code produced by this
/// independent implementation from the verbatim-copied key, then the existing enrolled apps keep
/// working after migration.
///
/// Matches Identity's authenticator settings: HMAC-SHA1, 30-second step, T0 = Unix epoch,
/// 6 digits, no modifier. The key is the Base32 string stored in AspNetUserTokens
/// (LoginProvider='[AspNetUserStore]', Name='AuthenticatorKey').
/// </summary>
public static class Authenticator
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string GenerateCode(string base32Key, DateTimeOffset? when = null)
    {
        var key = FromBase32(base32Key);
        var timestep = GetTimestep(when ?? DateTimeOffset.UtcNow);
        return ComputeTotp(key, timestep);
    }

    private static long GetTimestep(DateTimeOffset when)
    {
        var unixSeconds = when.ToUnixTimeSeconds();
        return unixSeconds / 30L;
    }

    private static string ComputeTotp(byte[] key, long timestep)
    {
        var counter = BitConverter.GetBytes(timestep);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counter); // RFC 6238 uses big-endian 8-byte counter

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counter);

        // Dynamic truncation (RFC 4226 §5.3)
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        var otp = binary % 1_000_000;
        return otp.ToString("D6");
    }

    private static byte[] FromBase32(string input)
    {
        input = input.TrimEnd('=').Replace(" ", "").ToUpperInvariant();
        var bits = 0;
        var value = 0;
        var output = new List<byte>(input.Length * 5 / 8);

        foreach (var c in input)
        {
            var idx = Base32Alphabet.IndexOf(c);
            if (idx < 0)
                throw new FormatException($"Invalid Base32 character '{c}'.");

            value = (value << 5) | idx;
            bits += 5;
            if (bits >= 8)
            {
                output.Add((byte)((value >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }

        return output.ToArray();
    }
}
