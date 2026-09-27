using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Web.Rooms;

/// <summary>
/// HTTP + WebSocket surface of listen-together rooms (spike S9):
/// <list type="bullet">
/// <item><c>POST /api/rooms</c> — signed-in only; creates a room hosted by the caller, returns <c>{ id }</c>.</item>
/// <item><c>GET /api/rooms/{id}</c> — anonymous; name/host/listener count (for a join page preview).</item>
/// <item><c>GET /ws/rooms/{id}?name=</c> — the room socket. Anonymous listeners are allowed (D31) and pass a
/// display name; a signed-in caller is identified by the auth cookie on the upgrade request.</item>
/// </list>
/// </summary>
public static class RoomEndpoints
{
    public static IEndpointRouteBuilder MapRooms(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/rooms", async (CreateRoomRequest? body, HttpContext http, RoomService rooms,
            UserManager<MintPlayerUser> users, CancellationToken ct) =>
        {
            if (http.User.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();
            var userId = users.GetUserId(http.User)!;
            var doc = await rooms.CreateAsync(userId, DisplayNameOf(http.User), body?.Name, ct);
            return doc is null
                ? Results.Problem($"You already host {RoomService.MaxRoomsPerHost} open rooms.", statusCode: StatusCodes.Status429TooManyRequests)
                : Results.Ok(new { id = doc.Id["Rooms/".Length..], doc.Name });
        });

        endpoints.MapGet("/api/rooms/{id}", async (string id, RoomService rooms) =>
            await rooms.GetSummaryAsync(id) is { } summary ? Results.Ok(summary) : Results.NotFound());

        endpoints.Map("/ws/rooms/{id}", async (string id, HttpContext http, RoomService rooms, UserManager<MintPlayerUser> users) =>
        {
            if (!http.WebSockets.IsWebSocketRequest)
            {
                http.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            // Cross-site WebSocket hijacking guard: browsers always send Origin on a WS upgrade and CORS does
            // not apply to WebSockets, so a foreign page could otherwise ride the auth cookie into a room.
            var origin = http.Request.Headers.Origin.ToString();
            if (origin.Length > 0 && (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri)
                || !string.Equals(originUri.Authority, http.Request.Host.Value, StringComparison.OrdinalIgnoreCase)))
            {
                http.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            string? userId = null;
            string displayName;
            if (http.User.Identity?.IsAuthenticated == true)
            {
                userId = users.GetUserId(http.User);
                displayName = DisplayNameOf(http.User);
            }
            else
            {
                var name = http.Request.Query["name"].ToString().Trim();
                displayName = name.Length == 0 ? "Guest" : name[..Math.Min(name.Length, 40)];
                displayName += " (guest)";
            }

            await rooms.HandleSocketAsync(http, id, userId, displayName);
        });

        return endpoints;
    }

    private static string DisplayNameOf(ClaimsPrincipal user)
    {
        var name = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Email) ?? "Host";
        var at = name.IndexOf('@');
        return at > 0 ? name[..at] : name;
    }

    public sealed record CreateRoomRequest(string? Name);
}
