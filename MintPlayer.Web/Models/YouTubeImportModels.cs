using MintPlayer.Web.YouTube;

namespace MintPlayer.Web.Models;

/// <summary><c>POST /api/playlist/import/youtube/preview</c> body.</summary>
public class YouTubeImportPreviewRequest
{
    /// <summary>Any YouTube URL carrying a playlist (<c>list=</c>) or a bare playlist id.</summary>
    public string? Url { get; set; }
}

/// <summary><c>POST /api/playlist/import/youtube</c> body.</summary>
public class YouTubeImportRequest
{
    public string? Url { get; set; }

    /// <summary>MintPlayer playlist name; defaults to the YouTube playlist title.</summary>
    public string? Name { get; set; }

    public bool IsPublic { get; set; }

    /// <summary>Create draft songs for available unmatched videos (Editor/Administrator only) and include them as tracks.</summary>
    public bool CreateDraftsForUnmatched { get; set; }
}

public class YouTubeMatchedTrack
{
    public int Position { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public string SongId { get; set; } = string.Empty;
    public string SongTitle { get; set; } = string.Empty;

    /// <summary>Other songs that carry the same video (catalog duplicates); null when unambiguous.</summary>
    public List<string>? OtherSongIds { get; set; }
}

public class YouTubeUnmatchedVideo
{
    public int Position { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ChannelTitle { get; set; }
    public TimeSpan? Duration { get; set; }

    /// <summary>False for deleted / private videos (no draft is ever created for those).</summary>
    public bool Available { get; set; } = true;
    public string? UnavailableReason { get; set; }

    /// <summary>Countries where the video is blocked (from <c>videos.list</c>), if any.</summary>
    public List<string>? BlockedRegions { get; set; }
}

public class YouTubeImportPreview
{
    public string PlaylistId { get; set; } = string.Empty;
    public string? PlaylistTitle { get; set; }
    public string? ChannelTitle { get; set; }
    public int TotalVideos { get; set; }
    public int MatchedCount => Matched.Count;
    public int UnmatchedCount => Unmatched.Count;
    public List<YouTubeMatchedTrack> Matched { get; set; } = [];
    public List<YouTubeUnmatchedVideo> Unmatched { get; set; } = [];

    /// <summary>Data API calls made for this response (0 when served from the short-lived cache).</summary>
    public List<YouTubeApiCall> ApiCalls { get; set; } = [];
    public int QuotaUnits => ApiCalls.Sum(c => c.Units);
}

public class YouTubeImportResult
{
    public string PlaylistId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    public int MatchedTracks { get; set; }
    public int DraftSongsCreated { get; set; }
    public int DraftTracks { get; set; }
    public int SkippedVideos { get; set; }
    public YouTubeImportPreview Preview { get; set; } = new();
}
