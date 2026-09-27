using MintPlayer.Migration.Model;
using MintPlayer.Migration.Transform;

namespace MintPlayer.Migration.Tests;

public class LyricsTransformTests
{
    private static LegacyLyrics Row(string text, string? timeline, int day)
        => new(1, Guid.NewGuid(), text, new DateTime(2021, 1, day), timeline);

    [Fact]
    public void Timeline_is_divided_by_20_and_reindexed_onto_all_lines()
    {
        const string text = "one\ntwo\n\nthree\n\nfour";
        var (normalized, starts, synced, dropped) = LyricsTransform.Reindex(text, [336, 490, 600, 700]);
        Assert.Equal(text, normalized);
        Assert.Equal(new double?[] { 16.8, 24.5, null, 30.0, null, 35.0 }, starts);
        Assert.Equal(4, synced);
        Assert.Equal(0, dropped);
    }

    [Fact]
    public void Partial_sync_pads_with_null()
    {
        var (_, starts, synced, _) = LyricsTransform.Reindex("a\n\nb\nc\nd", [20]);
        Assert.Equal(new double?[] { 1.0, null, null, null, null }, starts);
        Assert.Equal(1, synced);
    }

    [Fact]
    public void Surplus_entries_are_dropped_and_counted()
    {
        var (_, starts, _, dropped) = LyricsTransform.Reindex("a\nb", [20, 40, 60]);
        Assert.Equal(new double?[] { 1.0, 2.0 }, starts);
        Assert.Equal(1, dropped);
    }

    [Fact]
    public void Whitespace_only_line_counts_as_a_line_like_legacy_linify()
    {
        // linify keeps a line unless it is exactly "" — " " consumes a timeline entry
        var (_, starts, _, _) = LyricsTransform.Reindex("a\n \nb", [20, 40, 60]);
        Assert.Equal(new double?[] { 1.0, 2.0, 3.0 }, starts);
    }

    [Fact]
    public void Crlf_is_normalized_without_shifting_indexes()
    {
        // legacy splits on '\n' only, so "\r" is a non-empty line and keeps its entry
        var (text, starts, _, _) = LyricsTransform.Reindex("a\r\n\r\nb", [20, 40, 60]);
        Assert.Equal("a\n\nb", text);
        Assert.Equal(new double?[] { 1.0, 2.0, 3.0 }, starts);
    }

    [Fact]
    public void Collapse_takes_most_complete_timeline_not_latest()
    {
        var older = Row("a\nb\nc", "[20,40,60]", 1);
        var newer = Row("a\nb\nc", "[20]", 2);
        var result = LyricsTransform.Collapse([older, newer]);
        Assert.Equal(new double?[] { 1.0, 2.0, 3.0 }, result.StartTimes);
        Assert.False(result.TextsDiffered);
    }

    [Fact]
    public void Collapse_ties_go_to_the_latest_row_and_empty_timelines_are_ignored()
    {
        var result = LyricsTransform.Collapse([Row("a", "[20]", 1), Row("a", "[40]", 2), Row("a", "[]", 3), Row("a", "", 4)]);
        Assert.Equal(new double?[] { 2.0 }, result.StartTimes);
    }

    [Fact]
    public void Collapse_empty_text_yields_null_lyrics()
    {
        var result = LyricsTransform.Collapse([Row("", null, 1)]);
        Assert.Null(result.Text);
        Assert.Null(result.StartTimes);
    }

    [Fact]
    public void Collapse_reports_differing_texts()
        => Assert.True(LyricsTransform.Collapse([Row("a", null, 1), Row("b", null, 2)]).TextsDiffered);

    [Fact]
    public void Collapse_without_timeline_keeps_text_and_no_timing()
    {
        var result = LyricsTransform.Collapse([Row("a\nb", null, 1)]);
        Assert.Equal("a\nb", result.Text);
        Assert.Null(result.StartTimes);
    }

    /// <summary>S6: at every instant the new display highlights the same line legacy did.</summary>
    [Theory]
    [InlineData("one\ntwo\n\nthree\n\n\nfour\nfive", "[336,490,600,700,980]")]
    [InlineData("a\n\nb\nc\n\nd\ne\nf", "[20,40]")]            // partial sync
    [InlineData("a\n \nb\n\nc", "[20,40,60,80,100]")]          // whitespace line + surplus entry
    [InlineData("\n\na\nb\n", "[100,50]")]                     // leading blanks, non-monotonic
    public void Active_line_matches_legacy_at_every_instant(string text, string timeline)
    {
        var legacy = LyricsTransform.ParseTimeline(timeline);
        var (_, starts, _, _) = LyricsTransform.Reindex(text, legacy);
        // sample every 10 ms, offset by 5 ms so no sample sits exactly on a boundary
        // (legacy is "time < now", the new display "start <= now" — they differ only AT a boundary)
        for (var t = 0.005; t < 8; t += 0.01)
            Assert.Equal(LyricsTransform.LegacyActiveLine(text, legacy, t), LyricsTransform.NewActiveLine(starts, t));
    }
}
