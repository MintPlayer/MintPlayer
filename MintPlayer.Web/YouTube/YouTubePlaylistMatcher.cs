using MintPlayer.Domain.Entities;
using MintPlayer.Web.Models;

namespace MintPlayer.Web.YouTube;

/// <summary>
/// Pure matching step: playlist videos × candidate songs → report. A video matches a song when one of the
/// song's <see cref="Subject.Media"/> URLs has the same canonical video id (<see cref="YouTubeUrl.GetVideoId"/>).
/// Playlist order and duplicates are kept. When several songs carry the same video, the lowest song id wins
/// (deterministic) and the others are listed as <see cref="YouTubeMatchedTrack.OtherSongIds"/>.
/// </summary>
public static class YouTubePlaylistMatcher
{
    public static (List<YouTubeMatchedTrack> Matched, List<YouTubeUnmatchedVideo> Unmatched) Match(
        IEnumerable<YouTubePlaylistVideo> videos, IEnumerable<Song> candidateSongs)
    {
        var byVideoId = BuildIndex(candidateSongs);

        var matched = new List<YouTubeMatchedTrack>();
        var unmatched = new List<YouTubeUnmatchedVideo>();
        foreach (var video in videos)
        {
            if (byVideoId.TryGetValue(video.VideoId, out var songs))
            {
                matched.Add(new YouTubeMatchedTrack
                {
                    Position = video.Position,
                    VideoId = video.VideoId,
                    VideoTitle = video.Title,
                    SongId = songs[0].Id!,
                    SongTitle = songs[0].Title,
                    OtherSongIds = songs.Count > 1 ? songs.Skip(1).Select(s => s.Id!).ToList() : null,
                });
            }
            else
            {
                unmatched.Add(new YouTubeUnmatchedVideo
                {
                    Position = video.Position,
                    VideoId = video.VideoId,
                    Title = video.Title,
                    ChannelTitle = video.ChannelTitle,
                    Available = video.Available,
                    UnavailableReason = video.UnavailableReason,
                });
            }
        }
        return (matched, unmatched);
    }

    /// <summary>Canonical video id → songs carrying it (live songs only, ordered by id).</summary>
    public static Dictionary<string, List<Song>> BuildIndex(IEnumerable<Song> songs)
    {
        var index = new Dictionary<string, List<Song>>(StringComparer.Ordinal);
        foreach (var song in songs.Where(s => !s.IsDeleted && s.Id is not null).OrderBy(s => s.Id, StringComparer.Ordinal))
        {
            foreach (var videoId in song.Media.Select(m => YouTubeUrl.GetVideoId(m.Value)).OfType<string>().Distinct(StringComparer.Ordinal))
            {
                if (!index.TryGetValue(videoId, out var list))
                    index[videoId] = list = [];
                list.Add(song);
            }
        }
        return index;
    }
}
