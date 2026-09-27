using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Microsoft.Extensions.Options;

namespace MintPlayer.Web.YouTube;

/// <summary>Configuration section <c>YouTube</c>. The key comes from <c>YouTube:ApiKey</c> (env <c>YouTube__ApiKey</c>,
/// e.g. the server <c>.env</c>) or, failing that, from the file named by <c>YouTube:ApiKeyFile</c>.</summary>
public class YouTubeOptions
{
    public const string SectionName = "YouTube";

    public string? ApiKey { get; set; }
    public string? ApiKeyFile { get; set; }

    /// <summary>Safety cap on playlistItems pages (50 items each) — 5000 is YouTube's own playlist limit.</summary>
    public int MaxPages { get; set; } = 100;

    internal string? ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(ApiKey))
            return ApiKey.Trim();
        if (!string.IsNullOrWhiteSpace(ApiKeyFile) && File.Exists(ApiKeyFile))
            return File.ReadAllText(ApiKeyFile).Trim();
        return null;
    }
}

/// <summary>One Data API request, for quota accounting (list methods cost 1 unit regardless of <c>part</c>).</summary>
public record YouTubeApiCall(string Method, int Units);

/// <summary>A playlist entry as returned by <c>playlistItems.list</c>.</summary>
public record YouTubePlaylistVideo(int Position, string VideoId, string Title, string? ChannelTitle, bool Available, string? UnavailableReason);

public record YouTubePlaylistData(string PlaylistId, string? Title, string? ChannelTitle, IReadOnlyList<YouTubePlaylistVideo> Items);

public record YouTubeVideoDetails(string VideoId, TimeSpan? Duration, string? PrivacyStatus, bool Embeddable, IReadOnlyList<string> BlockedRegions);

/// <param name="Details">Details per video id that <c>videos.list</c> returned.</param>
/// <param name="Queried">Ids whose batch succeeded — an id here but not in <paramref name="Details"/> is gone (deleted/private).</param>
public record YouTubeVideoLookup(IReadOnlyDictionary<string, YouTubeVideoDetails> Details, IReadOnlySet<string> Queried);

public class YouTubeApiException(HttpStatusCode status, string reason, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
    /// <summary>Google's error reason, e.g. <c>playlistNotFound</c>, <c>quotaExceeded</c>, <c>keyInvalid</c>.</summary>
    public string Reason { get; } = reason;

    public bool IsQuotaOrKeyError => Reason is "quotaExceeded" or "dailyLimitExceeded" or "rateLimitExceeded"
        or "keyInvalid" or "keyExpired" or "accessNotConfigured" or "notConfigured" or "ipRefererBlocked";
}

public interface IYouTubeDataApi
{
    Task<YouTubePlaylistData> GetPlaylistAsync(string playlistId, IList<YouTubeApiCall> calls, CancellationToken cancellationToken);
    Task<YouTubeVideoLookup> GetVideosAsync(IReadOnlyCollection<string> videoIds, IList<YouTubeApiCall> calls, CancellationToken cancellationToken);
}

/// <summary>
/// Thin YouTube Data API v3 client. The API key travels in the <c>X-Goog-Api-Key</c> header — never in the
/// query string — so request URLs that reach HttpClient/ASP.NET logs never contain it.
/// </summary>
public class YouTubeDataApiClient(HttpClient http, IOptions<YouTubeOptions> options) : IYouTubeDataApi
{
    public const string BaseAddress = "https://www.googleapis.com/youtube/v3/";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<YouTubePlaylistData> GetPlaylistAsync(string playlistId, IList<YouTubeApiCall> calls, CancellationToken cancellationToken)
    {
        // playlists.list (1 unit): title for the default MintPlayer playlist name + a clean 404 for unknown ids.
        var meta = await GetAsync<ListResponse<PlaylistResource>>(
            $"playlists?part=snippet&id={Uri.EscapeDataString(playlistId)}&maxResults=1", "playlists.list", calls, cancellationToken);
        var playlist = meta.Items?.FirstOrDefault()
            ?? throw new YouTubeApiException(HttpStatusCode.NotFound, "playlistNotFound", "The playlist does not exist or is private.");

        var items = new List<YouTubePlaylistVideo>();
        string? pageToken = null;
        var pages = 0;
        do
        {
            var url = $"playlistItems?part=snippet,contentDetails,status&maxResults=50&playlistId={Uri.EscapeDataString(playlistId)}"
                    + (pageToken is null ? "" : $"&pageToken={Uri.EscapeDataString(pageToken)}");
            var page = await GetAsync<ListResponse<PlaylistItemResource>>(url, "playlistItems.list", calls, cancellationToken);
            foreach (var item in page.Items ?? [])
            {
                var videoId = item.ContentDetails?.VideoId ?? item.Snippet?.ResourceId?.VideoId;
                if (string.IsNullOrEmpty(videoId))
                    continue;

                // Deleted / private entries stay in the list as "Deleted video" / "Private video" without an owner.
                var privacy = item.Status?.PrivacyStatus;
                var available = item.Snippet?.VideoOwnerChannelId is not null && privacy is "public" or "unlisted" or null;
                string? reason = available ? null
                    : privacy is "private" ? "private"
                    : item.Snippet?.VideoOwnerChannelId is null ? "deleted or private"
                    : privacy;

                items.Add(new YouTubePlaylistVideo(
                    Position: items.Count,
                    VideoId: videoId,
                    Title: item.Snippet?.Title ?? videoId,
                    ChannelTitle: item.Snippet?.VideoOwnerChannelTitle,
                    Available: available,
                    UnavailableReason: reason));
            }
            pageToken = page.NextPageToken;
        } while (pageToken is not null && ++pages < options.Value.MaxPages);

        return new YouTubePlaylistData(playlistId, playlist.Snippet?.Title, playlist.Snippet?.ChannelTitle, items);
    }

    public async Task<YouTubeVideoLookup> GetVideosAsync(IReadOnlyCollection<string> videoIds, IList<YouTubeApiCall> calls, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, YouTubeVideoDetails>(StringComparer.Ordinal);
        var queried = new HashSet<string>(StringComparer.Ordinal);
        foreach (var chunk in videoIds.Distinct(StringComparer.Ordinal).Chunk(50))
        {
            ListResponse<VideoResource> page;
            try
            {
                page = await GetAsync<ListResponse<VideoResource>>(
                    $"videos?part=contentDetails,status&maxResults=50&id={Uri.EscapeDataString(string.Join(',', chunk))}",
                    "videos.list", calls, cancellationToken);
            }
            catch (YouTubeApiException ex) when (!ex.IsQuotaOrKeyError)
            {
                // Details are optional. Observed in S10: a batch of 50 valid ids can come back 403 "forbidden"
                // (location=myRating) — skip that batch rather than failing the whole preview.
                continue;
            }
            queried.UnionWith(chunk);
            foreach (var video in page.Items ?? [])
            {
                if (video.Id is null)
                    continue;
                TimeSpan? duration = null;
                if (video.ContentDetails?.Duration is { } iso)
                {
                    try { duration = XmlConvert.ToTimeSpan(iso); } catch (FormatException) { }
                }
                result[video.Id] = new YouTubeVideoDetails(
                    video.Id,
                    duration,
                    video.Status?.PrivacyStatus,
                    video.Status?.Embeddable ?? true,
                    video.ContentDetails?.RegionRestriction?.Blocked ?? []);
            }
        }
        return new YouTubeVideoLookup(result, queried);
    }

    private async Task<T> GetAsync<T>(string relativeUrl, string method, IList<YouTubeApiCall> calls, CancellationToken cancellationToken)
    {
        var key = options.Value.ResolveApiKey()
            ?? throw new YouTubeApiException(HttpStatusCode.ServiceUnavailable, "notConfigured", "The YouTube API key is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        request.Headers.Add("X-Goog-Api-Key", key);

        using var response = await http.SendAsync(request, cancellationToken);
        calls.Add(new YouTubeApiCall(method, 1)); // quota is charged for failed requests too

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ErrorEnvelope? error = null;
            try { error = await JsonSerializer.DeserializeAsync<ErrorEnvelope>(stream, Json, cancellationToken); }
            catch (JsonException) { }
            var reason = error?.Error?.Errors?.FirstOrDefault()?.Reason ?? response.StatusCode.ToString();
            throw new YouTubeApiException(response.StatusCode, reason, error?.Error?.Message ?? $"YouTube API returned {(int)response.StatusCode}.");
        }

        return await JsonSerializer.DeserializeAsync<T>(stream, Json, cancellationToken)
            ?? throw new YouTubeApiException(HttpStatusCode.BadGateway, "emptyResponse", "YouTube API returned an empty response.");
    }

    // --- Data API wire shapes (only the fields we read) ---
    private sealed class ListResponse<T>
    {
        public string? NextPageToken { get; set; }
        public List<T>? Items { get; set; }
    }

    private sealed class PlaylistResource
    {
        public PlaylistSnippet? Snippet { get; set; }
    }

    private sealed class PlaylistSnippet
    {
        public string? Title { get; set; }
        public string? ChannelTitle { get; set; }
    }

    private sealed class PlaylistItemResource
    {
        public PlaylistItemSnippet? Snippet { get; set; }
        public PlaylistItemContentDetails? ContentDetails { get; set; }
        public StatusPart? Status { get; set; }
    }

    private sealed class PlaylistItemSnippet
    {
        public string? Title { get; set; }
        public string? VideoOwnerChannelTitle { get; set; }
        public string? VideoOwnerChannelId { get; set; }
        public ResourceIdPart? ResourceId { get; set; }
    }

    private sealed class ResourceIdPart
    {
        public string? VideoId { get; set; }
    }

    private sealed class PlaylistItemContentDetails
    {
        public string? VideoId { get; set; }
    }

    private sealed class StatusPart
    {
        public string? PrivacyStatus { get; set; }
        public bool? Embeddable { get; set; }
    }

    private sealed class VideoResource
    {
        public string? Id { get; set; }
        public VideoContentDetails? ContentDetails { get; set; }
        public StatusPart? Status { get; set; }
    }

    private sealed class VideoContentDetails
    {
        public string? Duration { get; set; }
        public RegionRestrictionPart? RegionRestriction { get; set; }
    }

    private sealed class RegionRestrictionPart
    {
        [JsonPropertyName("blocked")]
        public List<string>? Blocked { get; set; }
    }

    private sealed class ErrorEnvelope
    {
        public ErrorBody? Error { get; set; }
    }

    private sealed class ErrorBody
    {
        public string? Message { get; set; }
        public List<ErrorItem>? Errors { get; set; }
    }

    private sealed class ErrorItem
    {
        public string? Reason { get; set; }
    }
}
