using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MintPlayer.Spark.Authorization.Identity;

namespace Spike.Migration;

/// <summary>
/// Decision D8 — migrate password hashes verbatim, no reset. Both the old MVC app and Spark use
/// ASP.NET Core Identity's default <see cref="PasswordHasher{TUser}"/> (PBKDF2 / "Identity V3",
/// NOT BCrypt). These tests prove a hash produced by the old app verifies through Spark's real
/// UserManager + RavenDB UserStore, and that older (lower-iteration) hashes are recognised as
/// upgradeable on login (forward-compatible).
/// </summary>
public class PasswordHashPortabilityTests : AuthSpikeBase
{
    private const string Password = "P@ssw0rd!";

    [Fact]
    public async Task Verbatim_hash_from_old_app_authenticates_in_Spark()
    {
        var sp = BuildIdentityServices();
        using var scope = sp.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SparkUser>>();

        // --- OLD APP: produce the hash that sits in AspNetUsers.PasswordHash today ---
        var oldHasher = new PasswordHasher<SparkUser>(); // default options == Spark's defaults
        var migrated = new SparkUser
        {
            UserName = "alice",
            NormalizedUserName = "ALICE",
            Email = "alice@example.com",
            NormalizedEmail = "ALICE@EXAMPLE.COM",
            SecurityStamp = "STAMP-COPIED-FROM-SQL",
        };
        migrated.PasswordHash = oldHasher.HashPassword(migrated, Password); // ETL copies this verbatim

        // --- ETL: write the user document ---
        string id;
        using (var store = new UserStore<SparkUser>(Store))
        {
            var create = await store.CreateAsync(migrated, CancellationToken.None);
            create.Succeeded.Should().BeTrue();
            id = migrated.Id!;
        }

        // --- SPARK SIDE: the user logs in with their existing password ---
        var user = await userManager.FindByIdAsync(id); // load-by-id == immediately consistent
        user.Should().NotBeNull();
        user!.PasswordHash.Should().Be(migrated.PasswordHash, "the hash is stored verbatim");

        (await userManager.CheckPasswordAsync(user, Password)).Should().BeTrue();
        (await userManager.CheckPasswordAsync(user, "wrong-password")).Should().BeFalse();
    }

    [Fact]
    public async Task Legacy_lower_iteration_hash_still_verifies_and_is_flagged_for_upgrade()
    {
        var sp = BuildIdentityServices();
        using var scope = sp.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SparkUser>>();

        // OLD APP configured with fewer PBKDF2 iterations than Spark's current default.
        var legacyHasher = new PasswordHasher<SparkUser>(
            Options.Create(new PasswordHasherOptions { IterationCount = 10_000 }));

        var migrated = new SparkUser
        {
            UserName = "bob",
            NormalizedUserName = "BOB",
            Email = "bob@example.com",
            NormalizedEmail = "BOB@EXAMPLE.COM",
        };
        var legacyHash = legacyHasher.HashPassword(migrated, Password);
        migrated.PasswordHash = legacyHash;

        string id;
        using (var store = new UserStore<SparkUser>(Store))
        {
            (await store.CreateAsync(migrated, CancellationToken.None)).Succeeded.Should().BeTrue();
            id = migrated.Id!;
        }

        var user = await userManager.FindByIdAsync(id);
        user.Should().NotBeNull();

        // Still authenticates across the iteration-count change.
        (await userManager.CheckPasswordAsync(user!, Password)).Should().BeTrue();

        // Forward-compatible: Spark's default hasher recognises the legacy hash as needing a
        // transparent re-hash to the current parameters on next login.
        var defaultHasher = new PasswordHasher<SparkUser>();
        defaultHasher.VerifyHashedPassword(user!, legacyHash, Password)
            .Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
    }
}
