using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
    /// <summary>Group <em>names</em> — must match a translation in App_Data/security.json.</summary>
    private const string AdministratorGroupName = "Administrator";
    private const string EditorGroupName = "Editor";

    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        await using var scope = app.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MintPlayerUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DevDataSeeder");

        // The admin exercises the full auto-UI; the editor exercises the trusted non-admin "Editor" group
        // (catalog + lyrics write without Administrator). Credentials are intentionally dev-only defaults.
        await EnsureUserAsync(userManager, logger,
            app.Configuration["DevSeed:AdminEmail"] ?? "admin@mintplayer.com",
            app.Configuration["DevSeed:AdminPassword"] ?? "Admin123!",
            AdministratorGroupName);

        await EnsureUserAsync(userManager, logger,
            app.Configuration["DevSeed:EditorEmail"] ?? "editor@mintplayer.com",
            app.Configuration["DevSeed:EditorPassword"] ?? "Editor123!",
            EditorGroupName);
    }

    /// <summary>
    /// Ensures a dev user exists with exactly the given group claim (idempotent). Drops any stale "group"
    /// claims with a different value, so re-running never leaves a user spanning two groups.
    /// </summary>
    private static async Task EnsureUserAsync(
        UserManager<MintPlayerUser> userManager, ILogger logger, string email, string password, string groupName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new MintPlayerUser { UserName = email, Email = email, EmailConfirmed = true };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                logger.LogWarning("Dev user seeding failed for {Email}: {Errors}",
                    email, string.Join("; ", create.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Seeded dev user {Email}", email);
        }

        var claims = await userManager.GetClaimsAsync(user);
        foreach (var stale in claims.Where(c => c.Type == "group" && c.Value != groupName))
        {
            await userManager.RemoveClaimAsync(user, stale);
            logger.LogInformation("Removed stale group claim '{Value}' from {Email}", stale.Value, email);
        }
        if (!claims.Any(c => c.Type == "group" && c.Value == groupName))
        {
            await userManager.AddClaimAsync(user, new Claim("group", groupName));
            logger.LogInformation("Granted {Group} group to {Email}", groupName, email);
        }
    }
}
