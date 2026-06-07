using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MintPlayer.Web;

/// <summary>
/// Development-only seeding. Ensures an Administrator account exists so the admin auto-UI
/// (e.g. the MediumType CRUD screens) can be exercised locally. Idempotent: safe to run on
/// every startup.
///
/// Group membership is expressed as a <c>"group"</c> claim whose value is the group's
/// <em>display name</em> (e.g. <c>"Administrator"</c>). Spark's default
/// <c>ClaimsGroupMembershipProvider</c> reads that claim, then <c>AccessControlService</c>
/// resolves the name back to the group id in <c>App_Data/security.json</c> by matching any of
/// the group's translations — so the claim must carry the name, NOT the group GUID. <c>.AddRoles</c>'s
/// principal factory surfaces stored user claims on the cookie principal, so no custom membership
/// provider is needed.
///
/// NOT for production: real admins are provisioned by the data migration / an explicit
/// bootstrap, never with a hard-coded default password.
/// </summary>
internal static class DevDataSeeder
{
    /// <summary>Administrator group <em>name</em> — must match a translation in App_Data/security.json.</summary>
    private const string AdministratorGroupName = "Administrator";

    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        // Credentials are configurable; the defaults are intentionally dev-only.
        var email = app.Configuration["DevSeed:AdminEmail"] ?? "admin@mintplayer.com";
        var password = app.Configuration["DevSeed:AdminPassword"] ?? "Admin123!";

        await using var scope = app.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MintPlayerUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DevDataSeeder");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new MintPlayerUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                logger.LogWarning("Dev admin seeding failed: {Errors}",
                    string.Join("; ", create.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Seeded dev administrator {Email}", email);
        }

        // Ensure exactly the Administrator group claim is present (idempotent). Drop any stale
        // "group" claims with a different value (e.g. an earlier GUID-valued attempt).
        var claims = await userManager.GetClaimsAsync(user);
        foreach (var stale in claims.Where(c => c.Type == "group" && c.Value != AdministratorGroupName))
        {
            await userManager.RemoveClaimAsync(user, stale);
            logger.LogInformation("Removed stale group claim '{Value}' from {Email}", stale.Value, email);
        }
        if (!claims.Any(c => c.Type == "group" && c.Value == AdministratorGroupName))
        {
            await userManager.AddClaimAsync(user, new Claim("group", AdministratorGroupName));
            logger.LogInformation("Granted Administrator group to {Email}", email);
        }
    }
}
