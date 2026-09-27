using MintPlayer.AspNetCore.SpaServices.Prerendering;
using MintPlayer.AspNetCore.SpaServices.Prerendering.Services;
using MintPlayer.AspNetCore.SpaServices.Routing;

namespace MintPlayer.Web.PublicSite;

/// <summary>
/// Server-side data for prerendered public pages (F2/F3, spike S1). <see cref="BuildRoutes"/> mirrors
/// the Angular public routes; <see cref="OnSupplyData"/> loads the entity straight from RavenDB so the
/// Node render needs no HTTP round-trip back into this app. Everything placed in <c>data</c> reaches
/// <c>main.server.ts</c> as <c>params.data</c> (camelCased by NodeServices' Newtonsoft settings).
/// </summary>
public sealed class MintPlayerSpaPrerenderingService : ISpaPrerenderingService
{
    private readonly ISpaRouteService spaRouteService;
    private readonly PublicSongReader songReader;

    public MintPlayerSpaPrerenderingService(ISpaRouteService spaRouteService, PublicSongReader songReader)
    {
        this.spaRouteService = spaRouteService;
        this.songReader = songReader;
    }

    public Task BuildRoutes(ISpaRouteBuilder routeBuilder)
    {
        routeBuilder.Group("song", "song", song => song
            .Route("{id}", "show"));
        return Task.CompletedTask;
    }

    public async Task OnSupplyData(HttpContext context, IDictionary<string, object> data)
    {
        var route = await spaRouteService.GetCurrentRoute(context);
        switch (route?.Name)
        {
            case "song-show":
                {
                    var song = await songReader.GetAsync(route.Parameters["id"], context.RequestAborted);
                    if (song is null)
                    {
                        // Prerendering (10.8+) keeps the status and still renders the "not found" view.
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                    }
                    else
                    {
                        data["song"] = song;
                    }
                }
                break;

            default:
                // Only public pages are server-rendered. The admin (Spark) UI stays client-rendered:
                // its Shell is auth-dependent and would render anonymously on the server, then mismatch.
                context.SkipPrerendering();
                break;
        }
    }
}
