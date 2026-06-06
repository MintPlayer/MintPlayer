using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MintPlayer.Spark.Authorization.Identity;
using MintPlayer.Spark.Testing;
using Raven.Client.Documents;

namespace Spike.Migration;

/// <summary>
/// Base for the auth round-trip tests. Spins up a real ASP.NET Core Identity
/// <see cref="UserManager{TUser}"/> backed by Spark's RavenDB <see cref="UserStore{TUser}"/> over
/// the embedded test store — the same stack the migrated app will run. This is what makes the
/// password and 2FA assertions faithful rather than reimplementations.
/// </summary>
public abstract class AuthSpikeBase : SparkTestDriver
{
    protected IServiceProvider BuildIdentityServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDocumentStore>(Store);
        services.AddLogging();
        services.AddDataProtection();

        services.AddIdentityCore<SparkUser>(options =>
            {
                // Defaults mirror the migrated app: default PBKDF2 (Identity V3) hasher,
                // default Authenticator (TOTP) token provider.
            })
            .AddDefaultTokenProviders();

        services.AddScoped<IUserStore<SparkUser>, UserStore<SparkUser>>();

        return services.BuildServiceProvider();
    }
}
