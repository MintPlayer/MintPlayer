using System.Diagnostics;
using System.Text.Json;
using MintPlayer.Migration;
using MintPlayer.Migration.Sources;
using MintPlayer.Migration.Target;
using MintPlayer.Migration.Transform;
using MintPlayer.Migration.Verify;

MigrationOptions options;
try
{
    options = MigrationOptions.Parse(args);
}
catch (HelpRequestedException)
{
    Console.WriteLine(MigrationOptions.Usage);
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine(MigrationOptions.Usage);
    return 64;
}

var log = Console.Out;
var total = Stopwatch.StartNew();
var phase = Stopwatch.StartNew();
void Phase(string name) { log.WriteLine($"[{phase.Elapsed.TotalSeconds,6:0.00}s] {name}"); phase.Restart(); }

var hashCodes = options.RecoveryCodes switch
{
    RecoveryCodeMode.Hashed => true,
    RecoveryCodeMode.Plain => false,
    _ => SparkConventions.UserStoreHashesRecoveryCodes(),
};
log.WriteLine($"MintPlayer.Migration — mode {options.Mode}, Spark {SparkConventions.SparkVersion()}, recovery codes {(hashCodes ? "SHA-256 hashed" : "verbatim")}, tz {options.TimeZone}");
if (options.MigratePasskeys)
    log.WriteLine("  --migrate-passkeys: nothing to do — production has no WebAuthnCredentials table (D13)");

var knownGroups = LoadGroupNames(options.SecurityJson);
var source = new SqlSnapshotSource(options.SqlConnectionString!, options.SqlSchema);
var snapshot = await source.LoadSnapshotAsync();
Phase($"read snapshot: {snapshot.Subjects.Count} subjects, {snapshot.Media.Count} media, {snapshot.Users.Count} users, {snapshot.Lyrics.Count} lyrics rows");

var preflight = Preflight.Check(snapshot, knownGroups);
if (preflight.Count > 0)
{
    Console.Error.WriteLine($"PRE-FLIGHT FAILED ({preflight.Count}):");
    foreach (var e in preflight) Console.Error.WriteLine("  " + e);
    return 2;
}
Phase("pre-flight ok");

var plan = LegacyTransformer.Transform(snapshot, new TransformOptions
{
    TimeZone = LegacyTime.Resolve(options.TimeZone),
    MigratedAt = DateTimeOffset.UtcNow,
    HashRecoveryCodes = hashCodes,
    KnownGroups = knownGroups,
});
Directory.CreateDirectory(options.OutDir);
await ReportWriter.WriteIssuesAsync(plan.Issues, Path.Combine(options.OutDir, "issues.txt"));
Phase($"transformed: {plan.Documents.Count} documents, {plan.CompareExchange.Count} reservations, {plan.Issues.Count} issues");
foreach (var g in plan.Issues.GroupBy(i => (i.Severity, i.Code)).OrderByDescending(g => g.Key.Severity).ThenBy(g => g.Key.Code))
    log.WriteLine($"    {g.Key.Severity,-7} {g.Key.Code,-28} x{g.Count()}");

if (options.Mode == RunMode.DryRun)
{
    await JsonlWriter.WriteAsync(plan, options.OutDir, log);
    Phase("dry run written");
    log.WriteLine($"total {total.Elapsed.TotalSeconds:0.00}s");
    return 0;
}

using var store = SparkConventions.CreateStore(options.RavenUrl, options.Database!);
if (options.Mode == RunMode.Run)
{
    var writer = new RavenWriter(store, log);
    await writer.RecreateDatabaseAsync();
    Phase("database recreated");
    await writer.WriteDocumentsAsync(plan);
    Phase("documents + compare-exchange written");
    await writer.DeployIndexesAsync(TimeSpan.FromMinutes(5));
    Phase("indexes deployed and non-stale");
}

var report = await new Reconciler(store, source, log).RunAsync(plan, options.OutDir, options.Samples, options.ExpectLive);
Phase("reconciled");
log.WriteLine($"total {total.Elapsed.TotalSeconds:0.00}s — reports in {Path.GetFullPath(options.OutDir)}");
return report.Green ? 0 : 1;

static IReadOnlySet<string> LoadGroupNames(string? path)
{
    path ??= FindUp("MintPlayer.Web/App_Data/security.json")
        ?? throw new FileNotFoundException("security.json not found; pass --security-json");
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var names = new HashSet<string>(StringComparer.Ordinal);
    foreach (var group in doc.RootElement.GetProperty("groups").EnumerateObject())
        foreach (var translation in group.Value.EnumerateObject())
            names.Add(translation.Value.GetString()!);
    return names;
}

static string? FindUp(string relative)
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate)) return candidate;
        }
    }
    return null;
}
