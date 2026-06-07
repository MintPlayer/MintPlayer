using System.Text.RegularExpressions;
using Microsoft.AspNetCore.HttpOverrides;
using MintPlayer.AspNetCore.SpaServices.Extensions;
using MintPlayer.Spark;
using MintPlayer.Spark.Authorization.Extensions;
using MintPlayer.Spark.Extensions;
using MintPlayer.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers();
builder.Services.AddSpark(builder.Configuration, spark =>
{
    spark.UseContext<MintPlayerSparkContext>();

    // Group-based access control from App_Data/security.json (deny-all by default;
    // the all-zeros "Everyone" group grants anonymous read where listed).
    spark.AddAuthorization(options => options.SecurityFilePath = "App_Data/security.json");

    // ASP.NET Core Identity over RavenDB (cookie + bearer, XSRF header X-XSRF-TOKEN).
    // Maps /spark/auth/* (login/register/forgot/reset/2fa). Migrated password hashes and
    // authenticator keys validate here unchanged (proven in spikes/Spike.Migration).
    spark.AddAuthentication<MintPlayerUser>();
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".SparkAuth.MintPlayer";
});

builder.Services.AddSpaStaticFilesImproved(configuration =>
{
    configuration.RootPath = "ClientApp/dist/ClientApp/browser";
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFilesImproved();

app.UseRouting();
app.UseSpark(o => o.SynchronizeModelsIfRequested<MintPlayerSparkContext>(args));

// Dev-only: ensure an Administrator account exists for the admin auto-UI. No-op in
// production and skipped during --spark-synchronize-model (UseSpark exits first).
await app.SeedDevelopmentDataAsync();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapSpark();
});

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/spark"),
    appBuilder =>
    {
        appBuilder.UseSpaImproved(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (app.Environment.IsDevelopment())
            {
                spa.UseAngularCliServer(npmScript: "start", cliRegexes: [openBrowserRegex()]);
            }
        });
    });

app.Run();

partial class Program
{
    [GeneratedRegex(@"Local\:\s+(?<openbrowser>https?\:\/\/(.+))")]
    private static partial Regex openBrowserRegex();
}
