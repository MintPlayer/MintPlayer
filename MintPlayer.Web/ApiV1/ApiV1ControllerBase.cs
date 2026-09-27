using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MintPlayer.Web.ApiV1;

/// <summary>
/// Shared plumbing of the legacy-compatible <c>api/v1</c> controllers (spike S7, F8/D5): the per-request
/// <see cref="ApiV1Catalog"/>, the <c>include_relations</c> header, and the caller — taken from the api/v1 JWT
/// when an <c>Authorization: Bearer</c> token is sent, otherwise from Spark's Identity cookie (legacy used the
/// cookie for its anonymous endpoints too).
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiV1ControllerBase : ControllerBase
{
    public const string AdministratorRole = "Administrator";
    /// <summary>D15: hidden medium types are visible to Administrator/Moderator (the future ViewHiddenMedia right).</summary>
    private static readonly string[] PrivilegedGroups = [AdministratorRole, "Moderator"];

    private bool userResolved;
    private MintPlayerUser? user;

    // Resolved from the request scope rather than injected through a base constructor: the repo's
    // MintPlayer.SourceGenerators InjectSourceGenerator generates constructors for derived classes of a base
    // with constructor parameters, which collides with primary constructors.
    protected ApiV1Catalog Catalog => HttpContext.RequestServices.GetRequiredService<ApiV1Catalog>();
    protected UserManager<MintPlayerUser> UserManager => HttpContext.RequestServices.GetRequiredService<UserManager<MintPlayerUser>>();

    /// <summary>The caller's principal: api/v1 JWT if presented and valid, else the ambient (cookie) user.</summary>
    protected async Task<ClaimsPrincipal?> GetPrincipalAsync()
    {
        if (Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var result = await HttpContext.AuthenticateAsync(ApiV1Jwt.Scheme);
            if (result.Succeeded) return result.Principal;
        }
        return User.Identity?.IsAuthenticated == true ? User : null;
    }

    /// <summary>The calling user document, or null when anonymous.</summary>
    protected async Task<MintPlayerUser?> GetUserAsync()
    {
        if (userResolved) return user;
        userResolved = true;
        var principal = await GetPrincipalAsync();
        if (principal is null) return null;

        var id = principal.FindFirstValue(ApiV1Jwt.UserIdClaim) ?? UserManager.GetUserId(principal);
        user = id is null ? null : await UserManager.FindByIdAsync(id);
        if (user is not null && await UserManager.IsLockedOutAsync(user)) user = null;
        return user;
    }

    /// <summary>Resolves the caller and sets <see cref="ApiV1Catalog.Privileged"/> (hidden media, D15).</summary>
    protected async Task PrepareAsync(CancellationToken ct)
    {
        var u = await GetUserAsync();
        if (u is not null)
        {
            var roles = await UserManager.GetRolesAsync(u);
            var groups = (await UserManager.GetClaimsAsync(u)).Where(c => c.Type == "group").Select(c => c.Value);
            Catalog.Privileged = roles.Concat(groups).Any(r => PrivilegedGroups.Contains(r, StringComparer.OrdinalIgnoreCase));
        }
        await Catalog.MediumTypesAsync(ct);
    }

    /// <summary>
    /// Legacy <c>[FromHeader] bool include_relations = false</c>. Model binding would reject a malformed value
    /// with 400; legacy did the same.
    /// </summary>
    protected bool IncludeRelations => HeaderFlag("include_relations", false);

    protected bool HeaderFlag(string name, bool fallback)
        => Request.Headers.TryGetValue(name, out var v) && bool.TryParse(v.ToString(), out var b) ? b : fallback;

    /// <summary>Legacy <c>User</c> DTO (UserMapper.Entity2Dto). Non-sensitive = what anonymous callers see of an author/owner.</summary>
    public static V1User MapUser(MintPlayerUser u, bool sensitive) => new()
    {
        Id = sensitive ? LegacyUserId(u.Id) : Guid.Empty.ToString(),
        UserName = u.UserName,
        Email = sensitive ? u.Email : null,
        IsTwoFactorEnabled = sensitive && u.TwoFactorEnabled,
        Bypass2faForExternalLogin = sensitive && u.Bypass2faForExternalLogin,
        // The migration turned the legacy "" into null (all 752 users); legacy wrote "".
        PictureUrl = u.PictureUrl ?? string.Empty,
    };

    /// <summary>Migrated users are <c>MintPlayerUsers/{legacyGuid:D}</c>; legacy clients expect the bare GUID.</summary>
    public static string? LegacyUserId(string? documentId)
    {
        if (documentId is null) return null;
        var slash = documentId.LastIndexOf('/');
        var tail = slash >= 0 ? documentId[(slash + 1)..] : documentId;
        return Guid.TryParse(tail, out var g) ? g.ToString("D") : documentId;
    }
}
