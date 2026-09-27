using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MintPlayer.Web.ApiV1;

/// <summary>
/// Configuration of the legacy-compatible JWT bearer scheme of <c>api/v1</c> (section <c>ApiV1:Jwt</c>;
/// mirrors legacy <c>JwtIssuerOptions</c>). Symmetric HMAC-SHA256, like legacy.
/// </summary>
public sealed class ApiV1JwtOptions
{
    public const string SectionName = "ApiV1:Jwt";

    public string Issuer { get; set; } = "https://mintplayer.com/";
    public string Audience { get; set; } = "Music";
    /// <summary>HMAC signing key (≥ 32 bytes UTF-8). Required outside Development/Staging; env <c>ApiV1__Jwt__Key</c>.</summary>
    public string? Key { get; set; }
    public TimeSpan ValidFor { get; set; } = TimeSpan.FromHours(2);
}

/// <summary>The resolved signing key. A placeholder random key is generated per process when none is configured.</summary>
public sealed class ApiV1SigningKey(SymmetricSecurityKey key, bool generated)
{
    public SymmetricSecurityKey Key { get; } = key;
    public bool Generated { get; } = generated;
}

public static class ApiV1Jwt
{
    /// <summary>A scheme of its own, next to Spark's Identity cookie/bearer — never the default scheme.</summary>
    public const string Scheme = "ApiV1Jwt";

    // Claim names as the legacy JwtSecurityTokenHandler wrote them (outbound short names).
    public const string UserIdClaim = "nameid";
    public const string UserNameClaim = "unique_name";
    public const string EmailClaim = "email";

    public static IServiceCollection AddApiV1(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var options = configuration.GetSection(ApiV1JwtOptions.SectionName).Get<ApiV1JwtOptions>() ?? new ApiV1JwtOptions();
        services.Configure<ApiV1JwtOptions>(configuration.GetSection(ApiV1JwtOptions.SectionName));

        SymmetricSecurityKey key;
        var generated = false;
        if (!string.IsNullOrEmpty(options.Key))
        {
            var bytes = Encoding.UTF8.GetBytes(options.Key);
            if (bytes.Length < 32)
                throw new InvalidOperationException($"{ApiV1JwtOptions.SectionName}:Key must be at least 32 bytes (HMAC-SHA256).");
            key = new SymmetricSecurityKey(bytes);
        }
        else if (environment.IsProduction())
        {
            throw new InvalidOperationException($"{ApiV1JwtOptions.SectionName}:Key is not configured (env ApiV1__Jwt__Key).");
        }
        else
        {
            // Dev/Staging placeholder: tokens die with the process.
            key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));
            generated = true;
        }
        services.AddSingleton(new ApiV1SigningKey(key, generated));
        services.AddScoped<ApiV1Catalog>();
        services.AddScoped<ApiV1TokenService>();

        services.AddAuthentication().AddJwtBearer(Scheme, jwt =>
        {
            jwt.MapInboundClaims = false;
            jwt.SaveToken = false;
            jwt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                NameClaimType = UserNameClaim,
                RoleClaimType = "role",
            };
        });
        return services;
    }
}

/// <summary>Mints the api/v1 token (legacy <c>AccountRepository.CreateToken</c>).</summary>
public sealed class ApiV1TokenService(IOptions<ApiV1JwtOptions> options, ApiV1SigningKey signingKey)
{
    public string CreateToken(MintPlayerUser user, IEnumerable<string> roles)
    {
        var o = options.Value;
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ApiV1Jwt.UserIdClaim, user.Id!),
            new(ApiV1Jwt.UserNameClaim, user.UserName ?? string.Empty),
        };
        if (!string.IsNullOrEmpty(user.Email)) claims.Add(new(ApiV1Jwt.EmailClaim, user.Email));
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(o.ValidFor),
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(signingKey.Key, SecurityAlgorithms.HmacSha256),
        };
        return new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false }.CreateToken(descriptor);
    }
}
