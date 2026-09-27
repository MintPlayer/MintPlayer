using MintPlayer.Migration.Transform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MintPlayer.Migration.Target;

/// <summary>
/// --dry-run output: one <c>{collection}.jsonl</c> per collection (each line the document with its
/// <c>@id</c>), <c>compare-exchange.jsonl</c> and <c>issues.txt</c>. NOTE: <c>MintPlayerUsers.jsonl</c> and
/// <c>compare-exchange.jsonl</c> contain personal data and credential hashes — keep the output directory
/// local and delete it after inspection.
/// </summary>
public static class JsonlWriter
{
    public static async Task WriteAsync(MigrationPlan plan, string directory, TextWriter log)
    {
        Directory.CreateDirectory(directory);
        var serializer = SparkConventions.CreateComparisonSerializer();
        foreach (var group in plan.Documents.GroupBy(d => d.Collection))
        {
            var path = Path.Combine(directory, $"{group.Key}.jsonl");
            await using var writer = new StreamWriter(path);
            foreach (var doc in group)
            {
                var json = JObject.FromObject(doc.Entity, serializer);
                json.AddFirst(new JProperty("@id", doc.Id));
                await writer.WriteLineAsync(json.ToString(Formatting.None));
            }
            log.WriteLine($"  {path} ({group.Count()} docs)");
        }

        var cmpx = Path.Combine(directory, "compare-exchange.jsonl");
        await using (var writer = new StreamWriter(cmpx))
        {
            foreach (var (key, value) in plan.CompareExchange)
                await writer.WriteLineAsync(new JObject { ["key"] = key, ["value"] = value }.ToString(Formatting.None));
        }
        log.WriteLine($"  {cmpx} ({plan.CompareExchange.Count} keys)");
        await ReportWriter.WriteIssuesAsync(plan.Issues, Path.Combine(directory, "issues.txt"));
    }
}

public static class ReportWriter
{
    public static async Task WriteIssuesAsync(IEnumerable<MigrationIssue> issues, string path)
    {
        await File.WriteAllLinesAsync(path, issues
            .OrderByDescending(i => i.Severity).ThenBy(i => i.Code).ThenBy(i => i.Subject, StringComparer.Ordinal)
            .Select(i => i.ToString()));
    }
}
