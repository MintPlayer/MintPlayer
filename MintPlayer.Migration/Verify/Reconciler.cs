using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MintPlayer.Domain.Entities;
using MintPlayer.Migration.Sources;
using MintPlayer.Migration.Target;
using MintPlayer.Migration.Transform;
using MintPlayer.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.CompareExchange;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Documents.Session;

namespace MintPlayer.Migration.Verify;

public sealed record CountRow(string Name, int Source, int Target, int ExplainedBy, string Note)
{
    public bool Ok => Source - ExplainedBy == Target;
}

public sealed class ReconcileReport
{
    public List<CountRow> Counts { get; } = [];
    public List<MigrationIssue> Findings { get; } = [];
    public int DocumentsCompared { get; set; }
    public int HashMatches { get; set; }
    public int ReferencesChecked { get; set; }
    public int UnresolvedReferences { get; set; }
    public int ReferencesToDeleted { get; set; }
    public Dictionary<string, int> LiveCounts { get; } = [];
    public List<LyricsTimingResult> Lyrics { get; } = [];
    public bool Green => Counts.All(c => c.Ok) && Findings.All(f => f.Severity != IssueSeverity.Error);

    public void Error(string code, string subject, string detail) => Findings.Add(new MigrationIssue(IssueSeverity.Error, code, subject, detail));
    public void Info(string code, string subject, string detail) => Findings.Add(new MigrationIssue(IssueSeverity.Info, code, subject, detail));
}

/// <summary>
/// §5.3 verification of a migrated database against its source:
/// (1) per-table vs per-collection counts, with source counts computed by independent SQL (the
/// soft-delete rule re-implemented in SQL, not taken from the transform) and every expected gap explained
/// by a transform issue code; (2) SHA-256 of every planned document vs the document re-read from RavenDB
/// (both through the same serializer); (3) every <c>*Id</c>/<c>*Ids</c>/like reference resolves;
/// (4) compare-exchange reservations resolve to users; (5) indexes non-stale and error-free; (6) sample
/// side-by-side dumps of public catalog documents; (7) optional expected live counts.
/// </summary>
public sealed class Reconciler(IDocumentStore store, SqlSnapshotSource source, TextWriter log)
{
    private const string Deleted = "EXISTS (SELECT 1 FROM {0}.AspNetUsers u WHERE u.Id = x.UserDeleteId)";

    private static readonly string[] CatalogCollections = ["Artists", "People", "Songs", "Tags", "TagCategories", "MediumTypes", "BlogPosts"];

    public async Task<ReconcileReport> RunAsync(MigrationPlan plan, Model.LegacySnapshot snapshot, string outDir, int samples, IReadOnlyDictionary<string, int> expectedLive, CancellationToken ct = default)
    {
        var report = new ReconcileReport();
        var serializer = SparkConventions.CreateComparisonSerializer();
        int Explained(params string[] codes) => plan.Issues.Count(i => codes.Contains(i.Code));

        // ---- load everything that is stored (typed by the planned CLR type of each collection)
        var stored = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in plan.Documents.GroupBy(d => (d.Collection, Type: d.Entity.GetType())))
        {
            var method = typeof(Reconciler).GetMethod(nameof(StreamCollectionAsync), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(group.Key.Type);
            var docs = await (Task<List<(string Id, object Entity)>>)method.Invoke(null, [store, group.Key.Collection + "/", ct])!;
            foreach (var (id, entity) in docs) stored[id] = entity;
        }
        var collectionStats = await store.Maintenance.SendAsync(new GetCollectionStatisticsOperation(), ct);

        // ---- (1) top-level counts
        async Task Top(string name, string totalSql, string? liveSql, string collection, Func<object, bool>? isLive, string liveKey)
        {
            var srcTotal = await source.CountAsync(totalSql, ct);
            var tgtTotal = collectionStats.Collections.GetValueOrDefault(collection) is { } n ? (int)n : 0;
            report.Counts.Add(new CountRow($"{name} (all)", srcTotal, tgtTotal, 0, $"collection {collection}"));
            if (liveSql != null && isLive != null)
            {
                var srcLive = await source.CountAsync(liveSql, ct);
                var tgtLive = stored.Where(kv => kv.Key.StartsWith(collection + "/", StringComparison.OrdinalIgnoreCase)).Count(kv => isLive(kv.Value));
                report.Counts.Add(new CountRow($"{name} (live)", srcLive, tgtLive, 0, "UserDeleteId joins an existing user = deleted"));
                report.LiveCounts[liveKey] = tgtLive;
            }
        }
        static bool Live(object e) => e is Entity { IsDeleted: false };

        await Top("Artists", "SELECT COUNT(*) FROM {0}.Subjects WHERE SubjectType = 'artist'",
            $"SELECT COUNT(*) FROM {{0}}.Subjects x WHERE SubjectType = 'artist' AND NOT {Deleted}", "Artists", Live, "artists");
        await Top("People", "SELECT COUNT(*) FROM {0}.Subjects WHERE SubjectType = 'person'",
            $"SELECT COUNT(*) FROM {{0}}.Subjects x WHERE SubjectType = 'person' AND NOT {Deleted}", "People", Live, "people");
        await Top("Songs", "SELECT COUNT(*) FROM {0}.Subjects WHERE SubjectType = 'song'",
            $"SELECT COUNT(*) FROM {{0}}.Subjects x WHERE SubjectType = 'song' AND NOT {Deleted}", "Songs", Live, "songs");
        await Top("MediumTypes", "SELECT COUNT(*) FROM {0}.MediumTypes",
            $"SELECT COUNT(*) FROM {{0}}.MediumTypes x WHERE NOT {Deleted}", "MediumTypes", Live, "mediumTypes");
        await Top("MediumTypes visible", "SELECT COUNT(*) FROM {0}.MediumTypes WHERE Visible = 1",
            $"SELECT COUNT(*) FROM {{0}}.MediumTypes x WHERE Visible = 1 AND NOT {Deleted}", "MediumTypes",
            e => e is MediumType { IsDeleted: false, Visible: true }, "visibleMediumTypes");
        report.Counts.RemoveAt(report.Counts.Count - 2); // "(all)" row of the visible pseudo-table duplicates MediumTypes
        await Top("Tags", "SELECT COUNT(*) FROM {0}.Tags", $"SELECT COUNT(*) FROM {{0}}.Tags x WHERE NOT {Deleted}", "Tags", Live, "tags");
        await Top("TagCategories", "SELECT COUNT(*) FROM {0}.TagCategories",
            $"SELECT COUNT(*) FROM {{0}}.TagCategories x WHERE NOT {Deleted}", "TagCategories", Live, "tagCategories");
        await Top("Playlists", "SELECT COUNT(*) FROM {0}.Playlists", "SELECT COUNT(*) FROM {0}.Playlists WHERE IsDeleted = 0", "Playlists", Live, "playlists");
        await Top("Playlists public", "SELECT COUNT(*) FROM {0}.Playlists WHERE Accessibility = 1",
            "SELECT COUNT(*) FROM {0}.Playlists WHERE Accessibility = 1 AND IsDeleted = 0", "Playlists",
            e => e is Playlist { IsDeleted: false, IsPublic: true }, "publicPlaylists");
        report.Counts.RemoveAt(report.Counts.Count - 2);
        await Top("BlogPosts", "SELECT COUNT(*) FROM {0}.BlogPosts", $"SELECT COUNT(*) FROM {{0}}.BlogPosts x WHERE NOT {Deleted}", "BlogPosts", Live, "blogPosts");
        await Top("Users", "SELECT COUNT(*) FROM {0}.AspNetUsers", null, "MintPlayerUsers", null, "users");
        await Top("UserLikes (users with likes)", "SELECT COUNT(DISTINCT UserId) FROM {0}.Likes", null, "UserLikes", null, "userLikes");

        var planned = plan.Documents.Count;
        var totalStored = (int)collectionStats.CountOfDocuments;
        report.Counts.Add(new CountRow("Documents (plan vs database)", planned, totalStored, 0, "no stray documents"));

        // ---- (1b) embedded rows
        var songs = stored.Values.OfType<Song>().ToList();
        var subjects = stored.Values.OfType<Subject>().ToList();
        var users = stored.Values.OfType<MintPlayerUser>().ToList();
        var likes = stored.Values.OfType<UserLike>().ToList();
        async Task Embedded(string name, string sql, int target, int explained = 0, string note = "")
            => report.Counts.Add(new CountRow(name, await source.CountAsync(sql, ct), target, explained, note));

        await Embedded("Media on existing subjects", "SELECT COUNT(*) FROM {0}.Media m JOIN {0}.Subjects s ON s.Id = m.SubjectId",
            subjects.Sum(x => x.Media.Count), Explained("medium-empty"), "gap = empty Value skipped");
        await Embedded("SubjectTag (distinct pairs)", "SELECT COUNT(*) FROM (SELECT DISTINCT SubjectId, TagId FROM {0}.SubjectTag) t",
            subjects.Sum(x => x.TagIds.Count));
        await Embedded("ArtistSong", "SELECT COUNT(*) FROM {0}.ArtistSong", songs.Sum(x => x.Artists.Count));
        await Embedded("ArtistSong uncredited", "SELECT COUNT(*) FROM {0}.ArtistSong WHERE Credited = 0", songs.Sum(x => x.Artists.Count(a => !a.Credited)));
        await Embedded("ArtistPerson", "SELECT COUNT(*) FROM {0}.ArtistPerson", stored.Values.OfType<Artist>().Sum(x => x.Members.Count));
        await Embedded("PlaylistSong", "SELECT COUNT(*) FROM {0}.PlaylistSong", stored.Values.OfType<Playlist>().Sum(x => x.Tracks.Count));
        await Embedded("Likes (like)", "SELECT COUNT(*) FROM {0}.Likes WHERE DoesLike = 1", likes.Sum(x => x.Likes.Count));
        await Embedded("Likes (dislike)", "SELECT COUNT(*) FROM {0}.Likes WHERE DoesLike = 0", likes.Sum(x => x.Dislikes.Count));
        await Embedded("Songs with lyrics text", "SELECT COUNT(DISTINCT SongId) FROM {0}.Lyrics WHERE LEN(LTRIM(RTRIM(Text))) > 0",
            songs.Count(x => x.Lyrics != null));
        await Embedded("Songs with a karaoke timeline",
            "SELECT COUNT(DISTINCT SongId) FROM {0}.Lyrics WHERE Timeline IS NOT NULL AND Timeline NOT IN ('', '[]')",
            songs.Count(x => x.LyricsTimings.Count > 0), Explained("timeline-no-playable") + plan.Issues.Count(i => i.Code == "lyrics-empty" && i.Detail.Contains("timeline")),
            "gap = no playable medium / empty text");
        await Embedded("User logins", "SELECT COUNT(*) FROM {0}.AspNetUserLogins", users.Sum(x => x.Logins.Count));
        await Embedded("User claims + roles→group claims",
            "SELECT (SELECT COUNT(*) FROM {0}.AspNetUserClaims) + (SELECT COUNT(*) FROM {0}.AspNetUserRoles)", users.Sum(x => x.Claims.Count));
        await Embedded("Users with password hash", "SELECT COUNT(*) FROM {0}.AspNetUsers WHERE PasswordHash IS NOT NULL", users.Count(x => x.PasswordHash != null));
        await Embedded("Users with 2FA enabled", "SELECT COUNT(*) FROM {0}.AspNetUsers WHERE TwoFactorEnabled = 1", users.Count(x => x.TwoFactorEnabled));
        await Embedded("Authenticator keys", "SELECT COUNT(*) FROM {0}.AspNetUserTokens WHERE LoginProvider = '[AspNetUserStore]' AND Name = 'AuthenticatorKey'",
            users.Count(x => x.AuthenticatorKey != null));
        await Embedded("Recovery-code sets", "SELECT COUNT(*) FROM {0}.AspNetUserTokens WHERE LoginProvider = '[AspNetUserStore]' AND Name = 'RecoveryCodes'",
            users.Count(x => x.TwoFactorRecoveryCodes.Count > 0));

        // ---- (4) compare-exchange
        var reservations = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var start = 0; ; start += 1024)
        {
            var page = await store.Operations.SendAsync(new GetCompareExchangeValuesOperation<string>("emails/", start, 1024), token: ct);
            foreach (var (key, value) in page) reservations[key] = value.Value;
            if (page.Count < 1024) break;
        }
        await Embedded("emails/ reservations", "SELECT COUNT(*) FROM {0}.AspNetUsers WHERE Email IS NOT NULL", reservations.Count);
        foreach (var (key, value) in reservations)
        {
            if (!stored.TryGetValue(value, out var u) || u is not MintPlayerUser user)
                report.Error("cmpx-unresolved", value, "reservation value is not a user document");
            else if (user.NormalizedEmail is null || Ids.EmailReservation(user.NormalizedEmail) != key)
                report.Error("cmpx-mismatch", value, "reservation key does not match the user's normalised email");
        }

        // ---- (2) SHA-256 round trip
        foreach (var doc in plan.Documents)
        {
            report.DocumentsCompared++;
            if (!stored.TryGetValue(doc.Id, out var entity))
            {
                report.Error("doc-missing", doc.Id, "planned document not in database");
                continue;
            }
            if (Sha256(doc.Entity, serializer) == Sha256(entity, serializer))
                report.HashMatches++;
            else
                report.Error("doc-hash-mismatch", doc.Id, "stored document differs from the transformed one");
        }

        // ---- (2b) S6: karaoke highlight replay against legacy, from the stored songs
        var storedSongs = stored.Where(kv => kv.Value is Song).ToDictionary(kv => kv.Key, kv => (Song)kv.Value, StringComparer.OrdinalIgnoreCase);
        report.Lyrics.AddRange(LyricsTimingCheck.Run(snapshot, storedSongs));
        foreach (var r in report.Lyrics.Where(r => r.Mismatches != 0 || !(r.MaxStartDelta <= 0.05)))
            report.Error("lyrics-timing", r.SongId, $"{r.Mismatches}/{r.Samples} samples highlight a different line, max start delta {r.MaxStartDelta:0.###}s");

        // ---- (3) reference integrity
        foreach (var (id, entity) in stored)
        {
            var json = JObject.FromObject(entity, serializer);
            foreach (var (path, reference) in References(json, isUserLikes: entity is UserLike))
            {
                report.ReferencesChecked++;
                if (!stored.TryGetValue(reference, out var target))
                {
                    report.UnresolvedReferences++;
                    report.Error("ref-unresolved", id, $"{path} → {reference}");
                }
                else if (target is Entity { IsDeleted: true } && entity is Entity { IsDeleted: false })
                {
                    report.ReferencesToDeleted++;
                    report.Info("ref-to-deleted", id, $"{path} → {reference} (soft-deleted)");
                }
            }
        }

        // ---- (5) indexes
        var stats = await store.Maintenance.SendAsync(new GetStatisticsOperation(), ct);
        if (stats.StaleIndexes.Length > 0) report.Error("index-stale", "indexes", string.Join(", ", stats.StaleIndexes));
        foreach (var e in await store.Maintenance.SendAsync(new GetIndexErrorsOperation(), ct))
            if (e.Errors.Length > 0) report.Error("index-errors", e.Name, $"{e.Errors.Length} errors");
        report.Info("indexes", "indexes", $"{stats.CountOfIndexes} indexes, {stats.StaleIndexes.Length} stale");

        // ---- (7) expected live counts (public API figures)
        foreach (var (key, expected) in expectedLive)
        {
            var actual = report.LiveCounts.GetValueOrDefault(key, -1);
            if (actual != expected) report.Error("live-count", key, $"expected {expected}, got {actual}");
            else report.Info("live-count", key, $"{actual} = expected");
        }

        // ---- (6) sample dumps (public catalog only — never users/playlists)
        Directory.CreateDirectory(outDir);
        var rng = new Random(20260927);
        var candidates = plan.Documents.Where(d => CatalogCollections.Contains(d.Collection)).ToList();
        var sb = new StringBuilder();
        foreach (var doc in candidates.OrderBy(_ => rng.Next()).Take(samples))
        {
            var a = Canonical(JObject.FromObject(doc.Entity, serializer));
            var b = stored.TryGetValue(doc.Id, out var e) ? Canonical(JObject.FromObject(e, serializer)) : "<missing>";
            sb.AppendLine($"=== {doc.Id}  {(a == b ? "EQUAL" : "DIFFERENT")}");
            sb.AppendLine("--- transformed"); sb.AppendLine(a);
            if (a != b) { sb.AppendLine("--- stored"); sb.AppendLine(b); }
        }
        await File.WriteAllTextAsync(Path.Combine(outDir, "samples.txt"), sb.ToString(), ct);
        await WriteReportAsync(report, Path.Combine(outDir, "reconcile.txt"));
        return report;
    }

    public async Task WriteReportAsync(ReconcileReport report, string path)
    {
        var lines = new List<string> { $"{"check",-40} {"source",7} {"target",7} {"expl.",6}  ok  note" };
        lines.AddRange(report.Counts.Select(c => $"{c.Name,-40} {c.Source,7} {c.Target,7} {c.ExplainedBy,6}  {(c.Ok ? "ok" : "XX")}  {c.Note}"));
        lines.Add("");
        lines.Add($"round-trip: {report.HashMatches}/{report.DocumentsCompared} SHA-256 equal");
        lines.Add($"references: {report.ReferencesChecked} checked, {report.UnresolvedReferences} unresolved, {report.ReferencesToDeleted} live→soft-deleted");
        lines.Add($"live: {string.Join(", ", report.LiveCounts.Select(kv => $"{kv.Key}={kv.Value}"))}");
        lines.Add("");
        lines.Add($"karaoke replay (S6): {report.Lyrics.Count} songs, {report.Lyrics.Sum(r => r.Samples)} samples @ {LyricsTimingCheck.Step * 1000:0} ms, " +
                  $"{report.Lyrics.Sum(r => Math.Max(0, r.Mismatches))} mismatches, max start delta {(report.Lyrics.Count == 0 ? 0 : report.Lyrics.Max(r => r.MaxStartDelta)):0.###} s");
        lines.AddRange(report.Lyrics.Select(r => $"  {r.SongId,-10} entries {r.LegacyEntries,3}  samples {r.Samples,6}  mismatches {r.Mismatches,3}  Δmax {r.MaxStartDelta:0.###}s  {r.Note}"));
        lines.Add("");
        lines.AddRange(report.Findings.OrderByDescending(f => f.Severity).Select(f => f.ToString()));
        lines.Add("");
        lines.Add(report.Green ? "RECONCILER: GREEN" : "RECONCILER: RED");
        await File.WriteAllLinesAsync(path, lines);
        foreach (var l in lines.Where(l => !l.StartsWith("Info ", StringComparison.Ordinal))) log.WriteLine("  " + l);
    }

    /// <summary>Every reference-looking value: properties named <c>*Id</c> (except Id/OldId) and <c>*Ids</c>,
    /// and on <c>UserLikes</c> the <c>Likes</c>/<c>Dislikes</c> subject-id arrays.</summary>
    internal static IEnumerable<(string Path, string Reference)> References(JToken token, bool isUserLikes)
    {
        foreach (var prop in token.SelectTokens("$..*").OfType<JValue>().Select(v => v.Parent).OfType<JProperty>().Distinct())
        {
            if (prop.Value is JValue { Type: JTokenType.String } v && IsRefName(prop.Name))
                yield return (prop.Path, (string)v!);
        }
        foreach (var array in token.SelectTokens("$..*").OfType<JArray>())
        {
            if (array.Parent is not JProperty p) continue;
            var isRefArray = p.Name.EndsWith("Ids", StringComparison.Ordinal) || (isUserLikes && p.Name is "Likes" or "Dislikes");
            if (!isRefArray) continue;
            foreach (var item in array.OfType<JValue>().Where(x => x.Type == JTokenType.String))
                yield return (item.Path, (string)item!);
        }
    }

    private static bool IsRefName(string name) => name.EndsWith("Id", StringComparison.Ordinal) && name is not "Id" and not "OldId";

    private static async Task<List<(string Id, object Entity)>> StreamCollectionAsync<T>(IDocumentStore store, string prefix, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var result = new List<(string, object)>();
        await using var stream = await session.Advanced.StreamAsync<T>(prefix, token: ct);
        while (await stream.MoveNextAsync())
            result.Add((stream.Current.Id, stream.Current.Document!));
        return result;
    }

    internal static string Sha256(object entity, JsonSerializer serializer)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(JObject.FromObject(entity, serializer)))));

    internal static string Canonical(JToken token) => Sort(token).ToString(Formatting.None);

    private static JToken Sort(JToken token) => token switch
    {
        JObject o => new JObject(o.Properties().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => new JProperty(p.Name, Sort(p.Value)))),
        JArray a => new JArray(a.Select(Sort)),
        _ => token.DeepClone(),
    };
}
