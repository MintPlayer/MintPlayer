using System.Text.Json;
using MintPlayer.Migration.Model;

namespace MintPlayer.Migration.Transform;

/// <summary>Result of collapsing a song's legacy lyrics rows onto the new model (D12, D29).</summary>
public sealed record LyricsResult(
    string? Text,
    IReadOnlyList<double?>? StartTimes,
    int LegacyEntries,
    int SyncedLines,
    int DroppedEntries,
    bool TextsDiffered);

/// <summary>
/// Legacy lyrics → <c>Song.Lyrics</c> + one <c>LyricsTiming</c>.
/// <para><b>Legacy layout</b> (verified in <c>legacy/.../linify.pipe.ts</c> + <c>app.component.ts</c>):
/// the timeline has one entry per line of <c>text.split('\n').filter(line =&gt; line !== '')</c> — the filter is
/// strict emptiness, so a whitespace-only line counts as a line. Values are stored ×20 as an <c>int[]</c> JSON
/// (EF value converter in <c>MintPlayerContext</c>).</para>
/// <para><b>New layout</b> (<c>ClientApp/src/app/lyrics/song-lyrics.ts</c>): <c>StartTimes[i]</c> is the start of
/// line <c>i</c> of <c>text.split('\n')</c> — <em>all</em> lines, blanks included; <c>null</c> = unsynced; the
/// display highlights the last line whose start has passed.</para>
/// So legacy entry <c>k</c> moves to the index of the k-th non-empty line; blank lines and lines past a
/// partial sync get <c>null</c>; entries beyond the last non-empty line (can't be displayed in legacy
/// either) are dropped and counted.
/// </summary>
public static class LyricsTransform
{
    public const double LegacyScale = 20.0;

    /// <summary>Parses the stored JSON <c>int[]</c>; null/empty/"[]" → empty.</summary>
    public static IReadOnlyList<int> ParseTimeline(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];
        return JsonSerializer.Deserialize<int[]>(json) ?? [];
    }

    /// <summary>
    /// Picks the song's single text and timeline: the text of the latest non-empty row (S2: the four songs
    /// with two rows have identical text — reported if not); the timeline with the most entries, ties to the
    /// latest row (D12: song 291's only timeline is on the older row).
    /// </summary>
    public static LyricsResult Collapse(IReadOnlyList<LegacyLyrics> rows)
    {
        if (rows.Count == 0)
            return new LyricsResult(null, null, 0, 0, 0, false);

        var byDate = rows.OrderBy(r => r.UpdatedAt).ToList();
        var nonEmptyTexts = byDate.Where(r => !string.IsNullOrWhiteSpace(r.Text)).Select(r => r.Text).ToList();
        var textsDiffered = nonEmptyTexts.Distinct(StringComparer.Ordinal).Count() > 1;
        var rawText = nonEmptyTexts.LastOrDefault();

        var timeline = byDate
            .Select((r, order) => (Timeline: ParseTimeline(r.Timeline), order))
            .Where(x => x.Timeline.Count > 0)
            .OrderByDescending(x => x.Timeline.Count)
            .ThenByDescending(x => x.order)
            .Select(x => x.Timeline)
            .FirstOrDefault() ?? [];

        if (rawText is null)
            return new LyricsResult(null, null, timeline.Count, 0, timeline.Count, textsDiffered);

        var (text, startTimes, synced, dropped) = Reindex(rawText, timeline);
        return new LyricsResult(text, timeline.Count == 0 ? null : startTimes, timeline.Count, synced, dropped, textsDiffered);
    }

    /// <summary>
    /// Re-indexes a legacy timeline (×20 ints, one per non-empty line) onto the all-lines layout.
    /// Returns the normalised text (CRLF → LF, computed per line after splitting on '\n' so line
    /// indexes never shift), the start times in seconds, the number of synced lines and the number of
    /// dropped legacy entries.
    /// </summary>
    public static (string Text, List<double?> StartTimes, int SyncedLines, int Dropped) Reindex(string rawText, IReadOnlyList<int> legacyTimeline)
    {
        var rawLines = rawText.Split('\n');
        var startTimes = new List<double?>(rawLines.Length);
        var k = 0;
        foreach (var rawLine in rawLines)
        {
            // Legacy's linify keeps a line unless it is exactly "" (a lone "\r" or " " is a line).
            if (rawLine.Length > 0 && k < legacyTimeline.Count)
            {
                startTimes.Add(legacyTimeline[k] / LegacyScale);
                k++;
            }
            else
            {
                if (rawLine.Length > 0)
                    k++; // non-empty line beyond a partial sync: consumes no entry, stays unsynced
                startTimes.Add(null);
            }
        }

        var nonEmptyLines = rawLines.Count(l => l.Length > 0);
        var synced = Math.Min(nonEmptyLines, legacyTimeline.Count);
        var dropped = Math.Max(0, legacyTimeline.Count - nonEmptyLines);
        var text = string.Join('\n', rawLines.Select(l => l.TrimEnd('\r')));
        return (text, startTimes, synced, dropped);
    }

    /// <summary>
    /// The legacy player's active line at time <paramref name="seconds"/>: the last non-empty line whose
    /// timeline entry is strictly before now (<c>value.time &lt; currentTime</c>). Returns the line index in
    /// the all-lines layout, or -1. Used by tests and the S6 spike to compare against <see cref="NewActiveLine"/>.
    /// </summary>
    public static int LegacyActiveLine(string rawText, IReadOnlyList<int> legacyTimeline, double seconds)
    {
        var lines = rawText.Split('\n');
        var active = -1;
        var k = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length == 0)
                continue;
            // legacy maps each non-empty line to timeline[k]; undefined (past the end) never passes
            if (k < legacyTimeline.Count && legacyTimeline[k] / LegacyScale < seconds)
                active = i;
            k++;
        }
        return active;
    }

    /// <summary>The new display's active line (<c>song-lyrics.ts activeLineIndex</c>): last i with start ≤ now.</summary>
    public static int NewActiveLine(IReadOnlyList<double?> startTimes, double seconds)
    {
        var active = -1;
        for (var i = 0; i < startTimes.Count; i++)
        {
            if (startTimes[i] is { } start && start <= seconds)
                active = i;
        }
        return active;
    }
}
