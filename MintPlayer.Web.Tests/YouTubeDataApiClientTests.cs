using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using MintPlayer.Web.YouTube;

namespace MintPlayer.Web.Tests;

public class YouTubeDataApiClientTests
{
    private const string FakeKey = "test-key-not-a-secret";

    private sealed class FakeHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> respond) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var (status, body) = respond(request);
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }

    private static (YouTubeDataApiClient, FakeHandler) Create(Func<HttpRequestMessage, (HttpStatusCode, string)> respond)
    {
        var handler = new FakeHandler(respond);
        var http = new HttpClient(handler) { BaseAddress = new Uri(YouTubeDataApiClient.BaseAddress) };
        return (new YouTubeDataApiClient(http, Options.Create(new YouTubeOptions { ApiKey = FakeKey })), handler);
    }

    private static string Item(string videoId, string title, string? owner = "Owner", string privacy = "public")
        => "{\"snippet\":{\"title\":\"" + title + "\","
         + (owner is null ? "" : $"\"videoOwnerChannelTitle\":\"{owner}\",\"videoOwnerChannelId\":\"UC{owner}\",")
         + "\"resourceId\":{\"videoId\":\"" + videoId + "\"}},"
         + "\"contentDetails\":{\"videoId\":\"" + videoId + "\"},\"status\":{\"privacyStatus\":\"" + privacy + "\"}}";

    [Fact]
    public async Task Pages_through_playlist_items_counting_one_unit_per_call_and_keeps_key_out_of_urls()
    {
        var (client, handler) = Create(req =>
        {
            var url = req.RequestUri!.ToString();
            if (url.Contains("/playlists?"))
                return (HttpStatusCode.OK, """{"items":[{"snippet":{"title":"My list","channelTitle":"Me"}}]}""");
            if (!url.Contains("pageToken="))
                return (HttpStatusCode.OK, $$"""{"nextPageToken":"P2","items":[{{Item("AAAAAAAAAAA", "A")}},{{Item("BBBBBBBBBBB", "Deleted video", owner: null, privacy: "privacyStatusUnspecified")}}]}""");
            return (HttpStatusCode.OK, $$"""{"items":[{{Item("CCCCCCCCCCC", "Private video", owner: null, privacy: "private")}},{{Item("AAAAAAAAAAA", "A again")}}]}""");
        });

        var calls = new List<YouTubeApiCall>();
        var data = await client.GetPlaylistAsync("PLabcdefghijkl", calls, CancellationToken.None);

        Assert.Equal("My list", data.Title);
        Assert.Equal(["AAAAAAAAAAA", "BBBBBBBBBBB", "CCCCCCCCCCC", "AAAAAAAAAAA"], data.Items.Select(i => i.VideoId));
        Assert.Equal([0, 1, 2, 3], data.Items.Select(i => i.Position));
        Assert.Equal([true, false, false, true], data.Items.Select(i => i.Available));
        Assert.Equal("private", data.Items[2].UnavailableReason);
        Assert.Equal(["playlists.list", "playlistItems.list", "playlistItems.list"], calls.Select(c => c.Method));
        Assert.Equal(3, calls.Sum(c => c.Units));

        Assert.All(handler.Requests, r =>
        {
            Assert.DoesNotContain(FakeKey, r.RequestUri!.ToString());
            Assert.DoesNotContain("key=", r.RequestUri!.Query);
            Assert.Equal(FakeKey, Assert.Single(r.Headers.GetValues("X-Goog-Api-Key")));
            Assert.Contains("maxResults=", r.RequestUri!.Query);
        });
    }

    [Fact]
    public async Task Unknown_playlist_throws_playlistNotFound_after_one_call()
    {
        var (client, _) = Create(_ => (HttpStatusCode.OK, """{"items":[]}"""));
        var calls = new List<YouTubeApiCall>();
        var ex = await Assert.ThrowsAsync<YouTubeApiException>(() => client.GetPlaylistAsync("PLdoesnotexist1", calls, CancellationToken.None));
        Assert.Equal("playlistNotFound", ex.Reason);
        Assert.Single(calls);
    }

    [Fact]
    public async Task Quota_errors_surface_google_reason()
    {
        var (client, _) = Create(_ => (HttpStatusCode.Forbidden,
            """{"error":{"code":403,"message":"quota","errors":[{"reason":"quotaExceeded"}]}}"""));
        var ex = await Assert.ThrowsAsync<YouTubeApiException>(() => client.GetPlaylistAsync("PLabcdefghijkl", [], CancellationToken.None));
        Assert.Equal("quotaExceeded", ex.Reason);
    }

    [Fact]
    public async Task Videos_are_batched_by_50_and_parse_duration_and_region_blocks()
    {
        var (client, handler) = Create(req =>
        {
            var ids = Uri.UnescapeDataString(req.RequestUri!.Query).Split("id=")[1].Split('&')[0].Split(',');
            var items = string.Join(',', ids.Select(id =>
                "{\"id\":\"" + id + "\",\"contentDetails\":{\"duration\":\"PT3M33S\",\"regionRestriction\":{\"blocked\":[\"DE\"]}},"
                + "\"status\":{\"privacyStatus\":\"public\",\"embeddable\":true}}"));
            return (HttpStatusCode.OK, $$"""{"items":[{{items}}]}""");
        });

        var ids = Enumerable.Range(0, 60).Select(i => $"v{i:D10}").ToList();
        var calls = new List<YouTubeApiCall>();
        var lookup = await client.GetVideosAsync(ids, calls, CancellationToken.None);

        Assert.Equal(2, calls.Count);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(60, lookup.Details.Count);
        Assert.Equal(60, lookup.Queried.Count);
        Assert.Equal(TimeSpan.FromSeconds(213), lookup.Details["v0000000000"].Duration);
        Assert.Equal(["DE"], lookup.Details["v0000000059"].BlockedRegions);
    }

    [Fact]
    public async Task A_forbidden_videos_batch_is_skipped_but_quota_errors_propagate()
    {
        var first = true;
        var (client, _) = Create(_ =>
        {
            if (first) { first = false; return (HttpStatusCode.Forbidden, """{"error":{"message":"myRating","errors":[{"reason":"forbidden"}]}}"""); }
            return (HttpStatusCode.OK, """{"items":[]}""");
        });
        var ids = Enumerable.Range(0, 60).Select(i => $"v{i:D10}").ToList();
        var lookup = await client.GetVideosAsync(ids, [], CancellationToken.None);
        Assert.Empty(lookup.Details);
        Assert.Equal(10, lookup.Queried.Count); // first batch (50) skipped; second batch returned nothing → gone

        var (quotaClient, _) = Create(_ => (HttpStatusCode.Forbidden, """{"error":{"errors":[{"reason":"quotaExceeded"}]}}"""));
        var ex = await Assert.ThrowsAsync<YouTubeApiException>(() => quotaClient.GetVideosAsync(ids, [], CancellationToken.None));
        Assert.Equal("quotaExceeded", ex.Reason);
    }
}
