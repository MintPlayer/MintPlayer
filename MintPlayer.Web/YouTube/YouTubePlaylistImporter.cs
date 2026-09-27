using Microsoft.Extensions.Caching.Memory;
using MintPlayer.Domain.Entities;
using MintPlayer.Web.Actions;
using MintPlayer.Web.Indexes;
using MintPlayer.Web.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.YouTube;

/// <summary>
/// Import a YouTube playlist (F16): fetch its videos via the Data API, match them to catalog songs by canonical
/// video id (<see cref="Songs_ByYouTubeId"/>), and create a MintPlayer <see cref="Playlist"/> — optionally with
/// draft songs for the unmatched videos.
///
/// <para>Quota: <c>playlists.list</c> 1 unit + <c>playlistItems.list</c> 1 unit per 50 videos + <c>videos.list</c>
/// 1 unit per 50 <i>unmatched</i> videos (duration / region blocks). The fetched playlist is cached for
/// <see cref="CacheDuration"/> so the preview → import round trip is charged once.</para>
/// </summary>
public class YouTubePlaylistImporter(IAsyncDocumentSession session, IYouTubeDataApi youTube, IMemoryCache cache)
{
    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    /// <summary>Tag attached to every draft song (fixed id → created once, filterable in the admin UI).</summary>
    public const string DraftTagId = "Tags/youtube-import-draft";
    public const string DraftTagDescription = "Draft (imported from YouTube)";

    /// <summary>Medium type given to a draft's YouTube URL (legacy id 1 in the production catalog).</summary>
    public const string DraftMediumTypeName = "Official Music Video";

    private const int IndexQueryChunk = 256;

    /// <param name="Gone">Ids videos.list was asked for but didn't return (deleted/private since being listed).</param>
    private sealed record CachedPlaylist(YouTubePlaylistData Data, Dictionary<string, YouTubeVideoDetails> Details, HashSet<string> Gone);

    public async Task<YouTubeImportPreview> PreviewAsync(string? url, CancellationToken cancellationToken)
    {
        if (!YouTubeUrl.TryGetPlaylistId(url, out var playlistId))
            throw new ArgumentException("Not a YouTube playlist URL (expected a link with list=… or a playlist id).");
        if (YouTubeUrl.IsUnsupportedPlaylist(playlistId))
            throw new ArgumentException("Auto-generated YouTube mixes (RD…) can't be imported; open the mix and save it as a playlist first.");

        var calls = new List<YouTubeApiCall>();
        var cacheKey = $"youtube-playlist:{playlistId}";
        if (!cache.TryGetValue(cacheKey, out CachedPlaylist? cached) || cached is null)
        {
            var data = await youTube.GetPlaylistAsync(playlistId, calls, cancellationToken);
            cached = new CachedPlaylist(data, new(StringComparer.Ordinal), new(StringComparer.Ordinal));
        }

        var songs = await FindSongsAsync(cached.Data.Items.Select(i => i.VideoId), cancellationToken);
        var (matched, unmatched) = YouTubePlaylistMatcher.Match(cached.Data.Items, songs);

        // Durations / region blocks only for the unmatched, available videos we haven't looked up yet.
        // Details are optional: a quota/key failure here still returns the (already paid-for) match report.
        var missing = unmatched
            .Where(u => u.Available && !cached.Details.ContainsKey(u.VideoId) && !cached.Gone.Contains(u.VideoId))
            .Select(u => u.VideoId).Distinct().ToList();
        if (missing.Count > 0)
        {
            try
            {
                var lookup = await youTube.GetVideosAsync(missing, calls, cancellationToken);
                lock (cached)
                {
                    foreach (var (id, details) in lookup.Details)
                        cached.Details[id] = details;
                    cached.Gone.UnionWith(lookup.Queried.Where(id => !lookup.Details.ContainsKey(id)));
                }
            }
            catch (YouTubeApiException) { }
        }
        foreach (var video in unmatched.Where(u => u.Available))
        {
            if (cached.Details.TryGetValue(video.VideoId, out var details))
            {
                video.Duration = details.Duration;
                video.BlockedRegions = details.BlockedRegions.Count > 0 ? details.BlockedRegions.ToList() : null;
            }
            else if (cached.Gone.Contains(video.VideoId))
            {
                // Listed in the playlist but videos.list doesn't return it: removed / made private since.
                video.Available = false;
                video.UnavailableReason = "not returned by videos.list";
            }
        }

        cache.Set(cacheKey, cached, CacheDuration);

        return new YouTubeImportPreview
        {
            PlaylistId = playlistId,
            PlaylistTitle = cached.Data.Title,
            ChannelTitle = cached.Data.ChannelTitle,
            TotalVideos = cached.Data.Items.Count,
            Matched = matched,
            Unmatched = unmatched,
            ApiCalls = calls,
        };
    }

    /// <param name="ownerId">The signed-in user (becomes <see cref="Playlist.OwnerId"/>).</param>
    /// <param name="mayCreateDrafts">Catalog-curation right (Editor/Administrator); drafts are refused otherwise.</param>
    public async Task<YouTubeImportResult> ImportAsync(YouTubeImportRequest request, string ownerId, bool mayCreateDrafts, CancellationToken cancellationToken)
    {
        if (request.CreateDraftsForUnmatched && !mayCreateDrafts)
            throw new UnauthorizedAccessException("Only editors can create draft songs.");

        var preview = await PreviewAsync(request.Url, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var trackSongIds = new SortedDictionary<int, string>();
        foreach (var m in preview.Matched)
            trackSongIds[m.Position] = m.SongId;

        var draftsCreated = 0;
        var draftTracks = 0;
        if (request.CreateDraftsForUnmatched)
        {
            var mediumTypeId = await session.Query<MediumType>()
                .Where(t => t.Name == DraftMediumTypeName && !t.IsDeleted)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);
            await EnsureDraftTagAsync(now, cancellationToken);

            var draftByVideo = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var video in preview.Unmatched.Where(u => u.Available))
            {
                if (!draftByVideo.TryGetValue(video.VideoId, out var songId))
                {
                    var song = new Song
                    {
                        Title = video.Title,
                        Media = [new Medium { Value = YouTubeUrl.WatchUrl(video.VideoId), TypeId = mediumTypeId }],
                        TagIds = [DraftTagId],
                        CreatedAt = now,
                    };
                    await session.StoreAsync(song, cancellationToken);
                    draftByVideo[video.VideoId] = songId = song.Id!;
                    draftsCreated++;
                }
                trackSongIds[video.Position] = songId;
                draftTracks++;
            }
        }

        var playlist = new Playlist
        {
            Name = string.IsNullOrWhiteSpace(request.Name) ? preview.PlaylistTitle ?? "YouTube import" : request.Name.Trim(),
            Description = $"Imported from YouTube playlist {preview.PlaylistId}"
                        + (preview.PlaylistTitle is null ? "" : $" \"{preview.PlaylistTitle}\""),
            IsPublic = request.IsPublic,
            CreatedAt = now,
            Tracks = trackSongIds.Values.Select(id => new PlaylistTrack { SongId = id }).ToList(),
        };
        PlaylistActions.StampOwner(playlist, ownerId);
        await session.StoreAsync(playlist, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);

        return new YouTubeImportResult
        {
            PlaylistId = playlist.Id!,
            Name = playlist.Name,
            TrackCount = playlist.Tracks.Count,
            MatchedTracks = preview.Matched.Count,
            DraftSongsCreated = draftsCreated,
            DraftTracks = draftTracks,
            SkippedVideos = preview.TotalVideos - playlist.Tracks.Count,
            Preview = preview,
        };
    }

    /// <summary>Live songs whose media carry any of the given video ids — one index query per 256 distinct ids.</summary>
    private async Task<List<Song>> FindSongsAsync(IEnumerable<string> videoIds, CancellationToken cancellationToken)
    {
        var songs = new List<Song>();
        foreach (var chunk in videoIds.Distinct(StringComparer.Ordinal).Chunk(IndexQueryChunk))
        {
            songs.AddRange(await session.Query<Songs_ByYouTubeId.Result, Songs_ByYouTubeId>()
                .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                .Where(r => r.YouTubeIds.ContainsAny(chunk))
                .OfType<Song>()
                .ToListAsync(cancellationToken));
        }
        return songs.DistinctBy(s => s.Id).ToList();
    }

    private async Task EnsureDraftTagAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var tag = await session.LoadAsync<Tag>(DraftTagId, cancellationToken);
        if (tag is null)
            await session.StoreAsync(new Tag { Description = DraftTagDescription, CreatedAt = now }, DraftTagId, cancellationToken);
        else if (tag.IsDeleted)
        {
            tag.IsDeleted = false;
            tag.DeletedAt = null;
        }
    }
}
