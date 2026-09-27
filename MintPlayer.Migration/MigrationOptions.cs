namespace MintPlayer.Migration;

public enum RunMode { DryRun, Run, VerifyOnly }

public enum RecoveryCodeMode { Auto, Hashed, Plain }

public sealed class MigrationOptions
{
    public RunMode Mode { get; set; } = RunMode.DryRun;
    public string Source { get; set; } = "sql";
    public string? SqlConnectionString { get; set; }
    public string SqlSchema { get; set; } = "mintplay";
    public string RavenUrl { get; set; } = "http://localhost:8080";
    public string? Database { get; set; }
    public string TimeZone { get; set; } = Transform.LegacyTime.DefaultTimeZoneId;
    public RecoveryCodeMode RecoveryCodes { get; set; } = RecoveryCodeMode.Auto;
    public string OutDir { get; set; } = "migration-output";
    public string? SecurityJson { get; set; }
    public Dictionary<string, int> ExpectLive { get; } = [];
    public int Samples { get; set; } = 10;
    public DateTimeOffset? MigratedAt { get; set; }
    public bool MigratePasskeys { get; set; }

    public const string Usage = """
        MintPlayer.Migration — legacy SQL Server snapshot → RavenDB (docs/PRD-Spark-Completion.md §5)

          --dry-run | --run | --verify-only   mode (default --dry-run). --run WIPES and recreates --database.
          --source sql                        only 'sql' (restored .bak) is implemented
          --sql "<connection string>"         SQL Server holding the restored snapshot (required)
          --sql-schema mintplay               schema of the legacy tables
          --raven http://localhost:8080       RavenDB server
          --database <name>                   target database (required for --run / --verify-only)
          --tz Europe/Amsterdam               zone of legacy DateTime.Now values (D19)
          --recovery-codes auto|hashed|plain  auto = match the referenced Spark UserStore
          --security-json <path>              App_Data/security.json (group names for roles)
          --out <dir>                         reports (+ JSONL on --dry-run; contains personal data!)
          --expect artists=138,people=9,...   assert live counts (keys: artists, people, songs,
                                              mediumTypes, visibleMediumTypes, tags, tagCategories,
                                              playlists, publicPlaylists, blogPosts)
          --samples 10                        side-by-side catalog samples in samples.txt
          --migrated-at <ISO instant>         CreatedAt for rows without audit dates (default: the
                                              snapshot as-of = its latest audit timestamp)
          --migrate-passkeys                  no-op: production has no WebAuthnCredentials table (D13)
        """;

    public static MigrationOptions Parse(string[] args)
    {
        var o = new MigrationOptions();
        for (var i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"{args[i]} needs a value");
            switch (args[i])
            {
                case "--dry-run": o.Mode = RunMode.DryRun; break;
                case "--run": o.Mode = RunMode.Run; break;
                case "--verify-only": o.Mode = RunMode.VerifyOnly; break;
                case "--source": o.Source = Next(); break;
                case "--sql": o.SqlConnectionString = Next(); break;
                case "--sql-schema": o.SqlSchema = Next(); break;
                case "--raven": o.RavenUrl = Next(); break;
                case "--database": o.Database = Next(); break;
                case "--tz": o.TimeZone = Next(); break;
                case "--recovery-codes": o.RecoveryCodes = Enum.Parse<RecoveryCodeMode>(Next(), ignoreCase: true); break;
                case "--security-json": o.SecurityJson = Next(); break;
                case "--out": o.OutDir = Next(); break;
                case "--samples": o.Samples = int.Parse(Next()); break;
                case "--migrate-passkeys": o.MigratePasskeys = true; break;
                case "--migrated-at": o.MigratedAt = DateTimeOffset.Parse(Next(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--expect":
                    foreach (var pair in Next().Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var kv = pair.Split('=');
                        o.ExpectLive[kv[0].Trim()] = int.Parse(kv[1]);
                    }
                    break;
                case "--help" or "-h": throw new HelpRequestedException();
                default: throw new ArgumentException($"Unknown argument '{args[i]}'");
            }
        }
        if (o.SqlConnectionString is null) throw new ArgumentException("--sql is required");
        if (o.Source != "sql") throw new ArgumentException($"--source {o.Source} is not implemented (only 'sql'; see RESULT.md)");
        if (o.Mode != RunMode.DryRun && string.IsNullOrWhiteSpace(o.Database)) throw new ArgumentException("--database is required for --run / --verify-only");
        return o;
    }
}

public sealed class HelpRequestedException : Exception;
