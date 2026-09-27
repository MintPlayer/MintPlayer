using System.Text.RegularExpressions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using MintPlayer.AspNetCore.SpaServices.Extensions;
using MintPlayer.AspNetCore.SpaServices.Prerendering;
using MintPlayer.AspNetCore.SpaServices.Routing;
using MintPlayer.Spark;
using MintPlayer.Spark.Authorization.Extensions;
using MintPlayer.Spark.Extensions;
using MintPlayer.Web;
using MintPlayer.Web.Email;

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

// Transactional email (account confirmation + password reset) via MailKit. Overrides Identity's
// no-op IEmailSender<TUser>; falls back to logging when no SMTP host is configured (dev).
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddTransient<IEmailSender<MintPlayerUser>, MintPlayerEmailSender>();

builder.Services.AddSpaStaticFilesImproved(configuration =>
{
    configuration.RootPath = "ClientApp/dist/ClientApp/browser";
});

// Server-side prerendering of public pages (D2, spike S1): route table + OnSupplyData (RavenDB).
builder.Services.AddScoped<MintPlayer.Web.PublicSite.PublicSongReader>();
builder.Services.AddSpaPrerenderingService<MintPlayer.Web.PublicSite.MintPlayerSpaPrerenderingService>();

// Prerendering needs the server bundle (dist/ClientApp/server, emitted by the production build) and
// `node` on PATH. On by default outside Development; in Development the SPA comes from `ng serve`
// and no server bundle exists, so opt in explicitly with Prerendering:Enabled=true after a prod build.
var prerenderingEnabled = builder.Configuration.GetValue("Prerendering:Enabled", !builder.Environment.IsDevelopment());

var app = builder.Build();

app.UseForwardedHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();
// Serve the pre-built SPA (ng build output) only in production. In development this would shadow
// the live Angular CLI dev-server (UseAngularCliServer below) with a stale dist/main.js, so edits
// never reach the browser — serve the SPA exclusively from the CLI dev-server while developing.
if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFilesImproved();
}

app.UseRouting();
app.UseSpark(o => o.SynchronizeModelsIfRequested<MintPlayerSparkContext>(args));

// Enable RavenDB revisions (Songs) so lyric edits are versioned. Runs after UseSpark, so it's
// skipped during --spark-synchronize-model; idempotent across boots.
await app.ConfigureRevisionsAsync();

// Dev-only: ensure an Administrator account exists for the admin auto-UI. No-op in
// production and skipped during --spark-synchronize-model (UseSpark exits first).
await app.SeedDevelopmentDataAsync();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapSpark();

    // AMP is dropped (D11): permanently redirect legacy AMP song URLs to the SSR'd song page.
    endpoints.MapGet("/amp/song/{id}", (string id) => Results.Redirect($"/song/{Uri.EscapeDataString(id)}", permanent: true));
});

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/spark")
            && !context.Request.Path.StartsWithSegments("/api"),
    appBuilder =>
    {
        appBuilder.UseSpaImproved(spa =>
        {
            spa.Options.SourcePath = "ClientApp";
            // With `ssr` enabled (production build config) the application builder emits the browser
            // shell as index.csr.html (never index.html), so static hosting can't shadow the server
            // render. `ng serve` (development config, no ssr) keeps serving index.html.
            if (!app.Environment.IsDevelopment())
            {
                spa.Options.DefaultPage = "/index.csr.html";
            }

            if (prerenderingEnabled)
            {
                // Registered before the dev-server proxy / default page, so their HTML is the template.
                spa.UseSpaPrerendering(options =>
                {
                    options.BootModulePath = $"{spa.Options.SourcePath}/dist/ClientApp/server/main.server.mjs";
                    options.BootModuleBuilder = null; // built at publish time (npm run build → ng build)
                    options.ExcludeUrls = ["/ngsw-worker.js", "/ngsw.json", "/manifest.webmanifest"];
                });
            }

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
