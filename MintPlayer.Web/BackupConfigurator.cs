using Raven.Client.Documents;
using Raven.Client.Documents.Operations.Backups;
using Raven.Client.ServerWide.Operations;

namespace MintPlayer.Web;

/// <summary>
/// Configuration for the RavenDB periodic backup task (PRD D23). Bound from the <c>RavenBackup</c>
/// section. The task is only ensured when <see cref="FolderPath"/> is set, so development (no section)
/// never creates one.
/// </summary>
public sealed class RavenBackupOptions
{
    public const string SectionName = "RavenBackup";

    /// <summary>Task name — the idempotency key: the task with this name is updated in place.</summary>
    public string TaskName { get; set; } = "mintplayer-periodic-backup";

    /// <summary>Folder INSIDE the RavenDB container (a compose volume). Empty = backups disabled.</summary>
    public string? FolderPath { get; set; }

    /// <summary>Cron, server time (UTC in the container). Default: nightly full at 02:00.</summary>
    public string FullBackupFrequency { get; set; } = "0 2 * * *";

    /// <summary>Cron. Default: hourly incremental at :15 (off the full-backup minute).</summary>
    public string IncrementalBackupFrequency { get; set; } = "15 * * * *";

    /// <summary>Backups older than this are deleted by RavenDB (applies to every destination).</summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Optional off-box destination: S3 or S3-compatible (Hetzner Object Storage via
    /// <c>CustomServerUrl</c> + <c>ForcePathStyle</c>). Bound straight onto Raven's own settings type.
    /// Cloud destinations are license-gated — see docs/spikes/S8-hetzner-deploy/RESULT.md.
    /// </summary>
    public S3Settings? S3 { get; set; }

    /// <summary>Optional off-box destination: FTP/FTPS (e.g. a Hetzner Storage Box). License-gated too.</summary>
    public FtpSettings? Ftp { get; set; }
}

/// <summary>
/// Ensures the RavenDB periodic backup task exists with the configured schedule (hourly incremental +
/// nightly full to a local folder, 30-day retention, optional off-box copy). Same shape as
/// <see cref="RevisionsConfigurator"/>: runs after <c>UseSpark</c> (so it is skipped during
/// <c>--spark-synchronize-model</c> and the database already exists) and is idempotent — the task is
/// looked up by name and only rewritten when its configuration differs, so an ordinary reboot leaves the
/// schedule and backup status untouched.
/// </summary>
internal static class BackupConfigurator
{
    public static async Task ConfigureBackupsAsync(this WebApplication app)
    {
        var options = app.Configuration.GetSection(RavenBackupOptions.SectionName).Get<RavenBackupOptions>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BackupConfigurator));
        if (string.IsNullOrWhiteSpace(options?.FolderPath))
        {
            logger.LogInformation("RavenDB periodic backup not configured ({Section}:FolderPath empty) — skipped", RavenBackupOptions.SectionName);
            return;
        }

        var store = app.Services.GetRequiredService<IDocumentStore>();
        var record = await store.Maintenance.Server.SendAsync(new GetDatabaseRecordOperation(store.Database));
        var existing = record?.PeriodicBackups?.FirstOrDefault(b => b.Name == options.TaskName);

        var desired = new PeriodicBackupConfiguration
        {
            Name = options.TaskName,
            BackupType = BackupType.Backup,
            FullBackupFrequency = options.FullBackupFrequency,
            IncrementalBackupFrequency = options.IncrementalBackupFrequency,
            LocalSettings = new LocalSettings { Disabled = false, FolderPath = options.FolderPath },
            RetentionPolicy = new RetentionPolicy
            {
                Disabled = false,
                MinimumBackupAgeToKeep = TimeSpan.FromDays(options.RetentionDays),
            },
            S3Settings = IsConfigured(options.S3) ? options.S3 : null,
            FtpSettings = IsConfigured(options.Ftp) ? options.Ftp : null,
        };

        if (existing is not null && Matches(existing, desired))
        {
            logger.LogInformation("RavenDB periodic backup task {Task} (id {TaskId}) is up to date", existing.Name, existing.TaskId);
            return;
        }

        if (existing is not null) desired.TaskId = existing.TaskId;
        var result = await store.Maintenance.SendAsync(new UpdatePeriodicBackupOperation(desired));
        logger.LogInformation("RavenDB periodic backup task {Task} {Action} (id {TaskId}) -> {Folder}, full '{Full}', incremental '{Incr}', retention {Days}d, S3 {S3}, FTP {Ftp}",
            desired.Name, existing is null ? "created" : "updated", result.TaskId, options.FolderPath,
            desired.FullBackupFrequency, desired.IncrementalBackupFrequency, options.RetentionDays,
            desired.S3Settings is not null, desired.FtpSettings is not null);
    }

    private static bool IsConfigured(S3Settings? s3) => !string.IsNullOrWhiteSpace(s3?.BucketName);
    private static bool IsConfigured(FtpSettings? ftp) => !string.IsNullOrWhiteSpace(ftp?.Url);

    private static bool Matches(PeriodicBackupConfiguration a, PeriodicBackupConfiguration b) =>
        a.BackupType == b.BackupType
        && !a.Disabled
        && a.FullBackupFrequency == b.FullBackupFrequency
        && a.IncrementalBackupFrequency == b.IncrementalBackupFrequency
        && a.LocalSettings is { Disabled: false } && a.LocalSettings.FolderPath == b.LocalSettings?.FolderPath
        && a.RetentionPolicy is { Disabled: false } && a.RetentionPolicy.MinimumBackupAgeToKeep == b.RetentionPolicy?.MinimumBackupAgeToKeep
        // Off-box destinations: compare presence + target only. Credentials are not compared (and never
        // logged); rotating a key therefore needs a changed target or a manual task edit — see RESULT.md.
        && (a.S3Settings is { Disabled: false } ? a.S3Settings.BucketName + "|" + a.S3Settings.RemoteFolderName + "|" + a.S3Settings.CustomServerUrl : null)
            == (b.S3Settings is null ? null : b.S3Settings.BucketName + "|" + b.S3Settings.RemoteFolderName + "|" + b.S3Settings.CustomServerUrl)
        && (a.FtpSettings is { Disabled: false } ? a.FtpSettings.Url : null) == b.FtpSettings?.Url;
}
