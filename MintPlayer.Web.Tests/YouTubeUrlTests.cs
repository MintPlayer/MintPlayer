using System.Text.RegularExpressions;
using MintPlayer.Web.YouTube;

namespace MintPlayer.Web.Tests;

public class YouTubeUrlTests
{
    private const string Id = "dQw4w9WgXcQ";

    [Theory]
    // Shapes found in the production catalog (S10 survey)
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=RickAstley")]
    // Other shapes users paste
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://music.youtube.com/watch?v=dQw4w9WgXcQ&feature=share")]
    [InlineData("https://www.youtube.com/watch?feature=youtu.be&v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?app=desktop&v=dQw4w9WgXcQ&t=42s")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLx0sYbCqOb8TBPRdmBHs5Iftvv9TPboYG&index=3")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ#t=30")]
    [InlineData("https://www.youtube.com/watch/?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?si=AbCdEf123&t=10")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?list=PLx0sYbCqOb8TBPRdmBHs5Iftvv9TPboYG")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ?start=30&autoplay=1")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ?feature=share")]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ?version=3")]
    [InlineData("HTTPS://WWW.YOUTUBE.COM/watch?v=dQw4w9WgXcQ")]
    [InlineData("  https://www.youtube.com/watch?v=dQw4w9WgXcQ  ")]
    [InlineData("dQw4w9WgXcQ")]
    public void GetVideoId_extracts_the_canonical_id(string url)
        => Assert.Equal(Id, YouTubeUrl.GetVideoId(url));

    [Fact]
    public void GetVideoId_preserves_case_and_accepts_hyphen_underscore()
        => Assert.Equal("a-B_c1D2e3F", YouTubeUrl.GetVideoId("https://youtu.be/a-B_c1D2e3F"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://en.wikipedia.org/wiki/Your_Love_(The_Outfield_song)")]
    [InlineData("https://vimeo.com/22424868")]
    [InlineData("https://www.dailymotion.com/video/x3q2iex")]
    [InlineData("https://soundcloud.com/oasisofficial/whatever")]
    [InlineData("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")]
    [InlineData("https://www.youtube.com/playlist?list=PLx0sYbCqOb8TBPRdmBHs5Iftvv9TPboYG")]
    [InlineData("https://www.youtube.com/watch?v=tooShort")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQX")]   // 12 chars — not an id
    [InlineData("https://notyoutube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?vv=dQw4w9WgXcQ")]
    public void GetVideoId_returns_null_for_non_video_urls(string? url)
        => Assert.Null(YouTubeUrl.GetVideoId(url));

    [Fact]
    public void Index_pattern_and_helper_agree()
    {
        // Songs_ByYouTubeId evaluates VideoIdPattern with plain Regex.Match server-side; make sure that
        // (without the bare-id shortcut) yields the same group as the helper for URL inputs.
        var match = Regex.Match("https://m.youtube.com/watch?v=dQw4w9WgXcQ&ab_channel=X", YouTubeUrl.VideoIdPattern);
        Assert.True(match.Success);
        Assert.Equal(Id, match.Groups[1].Value);
    }

    private const string Pl = "PLx0sYbCqOb8TBPRdmBHs5Iftvv9TPboYG";

    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=" + Pl)]
    [InlineData("https://youtube.com/playlist?list=" + Pl + "&si=abc123")]
    [InlineData("https://m.youtube.com/playlist?list=" + Pl)]
    [InlineData("https://music.youtube.com/playlist?list=" + Pl)]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=" + Pl + "&index=2")]
    [InlineData("https://www.youtube.com/watch?list=" + Pl + "&v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?list=" + Pl)]
    [InlineData("https://www.youtube.com/embed/videoseries?list=" + Pl)]
    [InlineData("www.youtube.com/playlist?list=" + Pl)]
    [InlineData(" " + Pl + " ")]
    public void GetPlaylistId_accepts_all_playlist_url_forms(string url)
        => Assert.Equal(Pl, YouTubeUrl.GetPlaylistId(url));

    [Theory]
    [InlineData("https://music.youtube.com/playlist?list=OLAK5uy_kkRgdpIRUOPDqlJb3pDqNIdCN8ds3hnKc", "OLAK5uy_kkRgdpIRUOPDqlJb3pDqNIdCN8ds3hnKc")]
    [InlineData("https://www.youtube.com/playlist?list=UUuAXFkgsw1L7xaCfnd5JJOw", "UUuAXFkgsw1L7xaCfnd5JJOw")]
    public void GetPlaylistId_accepts_album_and_uploads_lists(string url, string expected)
        => Assert.Equal(expected, YouTubeUrl.GetPlaylistId(url));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/playlist?list=")]
    [InlineData("https://www.youtube.com/playlist?list=WL")]
    [InlineData("https://example.com/playlist?list=" + Pl)]
    [InlineData("https://www.youtube.com/playlist?list=PL<script>")]
    [InlineData("dQw4w9WgXcQ")]   // a video id, not a playlist id
    public void GetPlaylistId_rejects_non_playlists(string? url)
        => Assert.Null(YouTubeUrl.GetPlaylistId(url));

    [Fact]
    public void Mixes_are_flagged_unsupported()
    {
        Assert.True(YouTubeUrl.IsUnsupportedPlaylist("RDdQw4w9WgXcQ"));
        Assert.False(YouTubeUrl.IsUnsupportedPlaylist(Pl));
    }
}
