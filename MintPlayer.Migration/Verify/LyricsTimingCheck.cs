using MintPlayer.Domain.Entities;
using MintPlayer.Migration.Model;
using MintPlayer.Migration.Transform;

namespace MintPlayer.Migration.Verify;

public sealed record LyricsTimingResult(
    string SongId,
    int LegacyEntries,
    int Samples,
    int Mismatches,
    double MaxStartDelta,
    bool DisplayedRowDiffers,
    string Note);

/// <summary>
/// S6 as a permanent check: for every song with a legacy timeline, replay playback in 10 ms steps and
/// compare the line legacy highlighted (<c>app.component.ts currentLyricsLine$</c> over the latest lyrics
/// row, which is what legacy displayed) with the line the new karaoke display highlights from the
/// <em>stored</em> <c>Song.LyricsTimings</c> (<c>song-lyrics.ts activeLineIndex</c>) — by line text.
/// Samples sit 5 ms off the 50 ms legacy grid so the only semantic difference (legacy "time &lt; now",
/// new "start ≤ now") never decides a sample; <see cref="LyricsTimingResult.MaxStartDelta"/> separately
/// proves each synced line starts at exactly the legacy instant.
/// </summary>
public static class LyricsTimingCheck
{
    public const double Step = 0.01;

    public static List<LyricsTimingResult> Run(LegacySnapshot snapshot, IReadOnlyDictionary<string, Song> storedSongs)
    {
        var results = new List<LyricsTimingResult>();
        foreach (var rows in snapshot.Lyrics.GroupBy(l => l.SongId).OrderBy(g => g.Key))
        {
            var withTimeline = rows.Where(r => LyricsTransform.ParseTimeline(r.Timeline).Count > 0).ToList();
            if (withTimeline.Count == 0)
                continue;

            var songId = Ids.Song(rows.Key);
            var displayed = rows.OrderBy(r => r.UpdatedAt).Last();            // what legacy showed
            var displayedTimeline = LyricsTransform.ParseTimeline(displayed.Timeline);
            // the row D12 migrates (most complete timeline, ties → latest) is the reference
            var reference = withTimeline.OrderByDescending(r => LyricsTransform.ParseTimeline(r.Timeline).Count).ThenByDescending(r => r.UpdatedAt).First();
            var legacyTimeline = LyricsTransform.ParseTimeline(reference.Timeline);
            var displayedDiffers = !displayedTimeline.SequenceEqual(legacyTimeline);

            if (!storedSongs.TryGetValue(songId, out var song) || song.LyricsTimings.Count == 0 || song.Lyrics is null)
            {
                results.Add(new LyricsTimingResult(songId, legacyTimeline.Count, 0, -1, double.NaN, displayedDiffers, "no stored timing"));
                continue;
            }

            var starts = song.LyricsTimings[0].StartTimes;
            var newLines = song.Lyrics.Split('\n');
            var legacyLines = reference.Text.Split('\n');

            // start-time delta of every synced line
            var maxDelta = 0.0;
            var k = 0;
            for (var i = 0; i < legacyLines.Length; i++)
            {
                if (legacyLines[i].Length == 0) continue;
                if (k < legacyTimeline.Count)
                {
                    var expected = legacyTimeline[k] / LyricsTransform.LegacyScale;
                    maxDelta = Math.Max(maxDelta, starts[i] is { } s ? Math.Abs(s - expected) : double.PositiveInfinity);
                }
                k++;
            }

            var end = legacyTimeline.Max() / LyricsTransform.LegacyScale + 5;
            int samples = 0, mismatches = 0;
            for (var t = Step / 2; t < end; t += Step)
            {
                samples++;
                var li = LyricsTransform.LegacyActiveLine(reference.Text, legacyTimeline, t);
                var ni = LyricsTransform.NewActiveLine(starts, t);
                var legacyText = li < 0 ? null : legacyLines[li].TrimEnd('\r');
                var newText = ni < 0 ? null : newLines[ni];
                if (li != ni || legacyText != newText)
                    mismatches++;
            }
            results.Add(new LyricsTimingResult(songId, legacyTimeline.Count, samples, mismatches, maxDelta, displayedDiffers,
                displayedDiffers ? $"legacy displayed the latest row ({displayedTimeline.Count} entries); D12 migrates the most complete ({legacyTimeline.Count})" : ""));
        }
        return results;
    }
}
