using MintPlayer.Domain.Entities;
using MintPlayer.Web.YouTube;

namespace MintPlayer.Web.Tests;

public class YouTubePlaylistMatcherTests
{
    private static Song Song(string id, string title, params string[] media) => new()
    {
        Id = id,
        Title = title,
        Media = media.Select(m => new Medium { Value = m, TypeId = "MediumTypes/1" }).ToList(),
    };

    private static YouTubePlaylistVideo Video(int position, string id, bool available = true)
        => new(position, id, $"Video {id}", "Channel", available, available ? null : "deleted or private");

    [Fact]
    public void Matches_across_catalog_url_variants_keeping_order_and_duplicates()
    {
        var songs = new[]
        {
            Song("Songs/1", "One", "https://www.youtube.com/watch?v=AAAAAAAAAAA"),
            Song("Songs/2", "Two", "https://m.youtube.com/watch?v=BBBBBBBBBBB", "https://en.wikipedia.org/wiki/Two"),
            Song("Songs/3", "Three", "https://youtu.be/CCCCCCCCCCC"),
            Song("Songs/4", "Four", "https://www.youtube.com/watch?v=DDDDDDDDDDD&ab_channel=Four"),
        };
        var videos = new[]
        {
            Video(0, "DDDDDDDDDDD"), Video(1, "XXXXXXXXXXX"), Video(2, "AAAAAAAAAAA"),
            Video(3, "CCCCCCCCCCC"), Video(4, "BBBBBBBBBBB"), Video(5, "AAAAAAAAAAA"),
        };

        var (matched, unmatched) = YouTubePlaylistMatcher.Match(videos, songs);

        Assert.Equal(["Songs/4", "Songs/1", "Songs/3", "Songs/2", "Songs/1"], matched.Select(m => m.SongId));
        Assert.Equal([0, 2, 3, 4, 5], matched.Select(m => m.Position));
        var miss = Assert.Single(unmatched);
        Assert.Equal("XXXXXXXXXXX", miss.VideoId);
        Assert.Equal(1, miss.Position);
    }

    [Fact]
    public void Video_ids_are_case_sensitive()
    {
        var songs = new[] { Song("Songs/1", "One", "https://youtu.be/abcdefghijk") };
        var (matched, unmatched) = YouTubePlaylistMatcher.Match([Video(0, "ABCDEFGHIJK")], songs);
        Assert.Empty(matched);
        Assert.Single(unmatched);
    }

    [Fact]
    public void Deleted_songs_never_match()
    {
        var song = Song("Songs/1", "One", "https://youtu.be/AAAAAAAAAAA");
        song.IsDeleted = true;
        var (matched, _) = YouTubePlaylistMatcher.Match([Video(0, "AAAAAAAAAAA")], [song]);
        Assert.Empty(matched);
    }

    [Fact]
    public void Ambiguous_video_picks_lowest_id_and_reports_the_rest()
    {
        var songs = new[]
        {
            Song("Songs/9", "Nine", "https://youtu.be/AAAAAAAAAAA"),
            Song("Songs/10", "Ten", "https://www.youtube.com/watch?v=AAAAAAAAAAA"),
        };
        var track = Assert.Single(YouTubePlaylistMatcher.Match([Video(0, "AAAAAAAAAAA")], songs).Matched);
        Assert.Equal("Songs/10", track.SongId); // ordinal: "Songs/10" < "Songs/9"
        Assert.Equal(["Songs/9"], track.OtherSongIds!);
    }

    [Fact]
    public void Same_video_twice_on_one_song_is_not_ambiguous()
    {
        var songs = new[] { Song("Songs/1", "One", "https://youtu.be/AAAAAAAAAAA", "https://m.youtube.com/watch?v=AAAAAAAAAAA") };
        var track = Assert.Single(YouTubePlaylistMatcher.Match([Video(0, "AAAAAAAAAAA")], songs).Matched);
        Assert.Null(track.OtherSongIds);
    }

    [Fact]
    public void Unavailable_videos_are_reported_unmatched_with_reason()
    {
        var (_, unmatched) = YouTubePlaylistMatcher.Match([Video(0, "AAAAAAAAAAA", available: false)], []);
        var video = Assert.Single(unmatched);
        Assert.False(video.Available);
        Assert.Equal("deleted or private", video.UnavailableReason);
    }
}
