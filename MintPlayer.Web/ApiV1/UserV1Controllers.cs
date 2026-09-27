using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Domain.Entities;
using MintPlayer.Web.Indexes;
using MintPlayer.Web.Projections;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace MintPlayer.Web.ApiV1;

/// <summary>Read model over the <c>BlogPosts</c> collection (the migration's stopgap type until F1 lands).</summary>
public sealed class V1BlogPostDocument : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? AuthorId { get; set; }
    public DateTimeOffset? Published { get; set; }
}

[Route("api/v1/account")]
public sealed class AccountV1Controller(
    SignInManager<MintPlayerUser> signInManager,
    ApiV1TokenService tokens)
    : ApiV1ControllerBase
{
    /// <summary>
    /// AccountRepository.LocalLogin(createCookie: false): password check (with lockout), confirmed e-mail,
    /// then a token — or <c>status = 2</c> (RequiresTwoFactor) without a token for 2FA accounts, since v1 has
    /// no 2FA step. Accepts an e-mail address (legacy) or, as a fallback, the user name (S3 finding).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<V1LocalLoginResult>> Login([FromBody] V1LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password)) return Unauthorized();

        var user = await UserManager.FindByEmailAsync(request.Email) ?? await UserManager.FindByNameAsync(request.Email);
        if (user is null) return Unauthorized();

        var check = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!check.Succeeded) return Unauthorized();
        // Legacy threw EmailNotConfirmedException here, which surfaced as a 500.
        if (!await UserManager.IsEmailConfirmedAsync(user)) return Unauthorized();

        if (user.TwoFactorEnabled)
            return Ok(new V1LocalLoginResult { Status = 2, User = MapUser(user, sensitive: true) });

        var roles = await UserManager.GetRolesAsync(user);
        return Ok(new V1LocalLoginResult
        {
            Status = 1,
            User = MapUser(user, sensitive: true),
            Token = tokens.CreateToken(user, roles),
        });
    }

    [HttpGet("current-user")]
    [Authorize(AuthenticationSchemes = ApiV1Jwt.Scheme)]
    public async Task<ActionResult<V1User>> CurrentUser()
    {
        var user = await GetUserAsync();
        return user is null ? Unauthorized() : Ok(MapUser(user, sensitive: true));
    }

    /// <summary>Legacy returned role names (strings), not the Swagger <c>Role</c> objects.</summary>
    [HttpGet("roles")]
    [Authorize(AuthenticationSchemes = ApiV1Jwt.Scheme)]
    public async Task<ActionResult<IEnumerable<string>>> Roles()
    {
        var user = await GetUserAsync();
        return user is null ? Unauthorized() : Ok(await UserManager.GetRolesAsync(user));
    }
}

[Route("api/v1/playlist")]
public sealed class PlaylistV1Controller
    : ApiV1ControllerBase
{
    /// <summary>PlaylistRepository.GetPlaylists(Public).</summary>
    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<V1Playlist>>> Public(CancellationToken ct)
    {
        await PrepareAsync(ct);
        var playlists = await Catalog.Session.Query<Playlist>().Where(p => p.IsPublic).ToListAsync(ct);
        return Ok(await MapListAsync(playlists, ct));
    }

    /// <summary>PlaylistRepository.GetPlaylists(My) — JWT only, like legacy.</summary>
    [HttpGet("my")]
    [Authorize(AuthenticationSchemes = ApiV1Jwt.Scheme)]
    public async Task<ActionResult<IEnumerable<V1Playlist>>> My(CancellationToken ct)
    {
        await PrepareAsync(ct);
        var user = await GetUserAsync();
        if (user is null) return Unauthorized();
        var playlists = await Catalog.Session.Query<Playlist>().Where(p => p.OwnerId == user.Id).ToListAsync(ct);
        return Ok(await MapListAsync(playlists, ct));
    }

    /// <summary>
    /// PlaylistRepository.GetPlaylist: public → anyone; private → 401 anonymous, 403 not the owner.
    /// With relations the tracks carry media and artists (full <c>description</c>), without lyrics.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1Playlist>> Get(int id, CancellationToken ct)
    {
        await PrepareAsync(ct);
        var playlist = await Catalog.LoadLiveAsync<Playlist>("Playlists", id, ct);
        if (playlist is null) return NotFound();
        if (!playlist.IsPublic)
        {
            var user = await GetUserAsync();
            if (user is null) return Unauthorized();
            if (!string.Equals(user.Id, playlist.OwnerId, StringComparison.OrdinalIgnoreCase)) return StatusCode(StatusCodes.Status403Forbidden);
        }
        var rel = IncludeRelations;
        if (rel) await Catalog.WarmAsync(ct);
        return Ok(await MapAsync(playlist, rel, new ApiV1Catalog.SongLoad(false, true, false, null)));
    }

    private async Task<List<V1Playlist>> MapListAsync(IEnumerable<Playlist> playlists, CancellationToken ct)
    {
        var rel = IncludeRelations;
        if (rel) await Catalog.WarmAsync(ct);
        var result = new List<V1Playlist>();
        foreach (var p in playlists.Where(p => !p.IsDeleted).OrderBy(ApiV1Catalog.LegacyId))
            result.Add(await MapAsync(p, rel, ApiV1Catalog.SongLoad.MediaOnly));
        return result;
    }

    private async Task<V1Playlist> MapAsync(Playlist p, bool rel, ApiV1Catalog.SongLoad trackLoad)
    {
        var dto = new V1Playlist
        {
            Id = ApiV1Catalog.LegacyId(p),
            Description = p.Name,
            Accessibility = p.IsPublic ? 1 : 0,
        };
        if (!rel) return dto;
        var owner = p.OwnerId is null ? null : await Catalog.Session.LoadAsync<MintPlayerUser>(p.OwnerId);
        dto.User = owner is null ? null : MapUser(owner, sensitive: false);
        dto.Tracks = p.Tracks
            .Select(t => Catalog.FindSong(t.SongId))
            .OfType<Song>()
            .Select(s => Catalog.MapSong(s, trackLoad))
            .ToList();
        return dto;
    }
}

[Route("api/v1/blogpost")]
public sealed class BlogPostV1Controller
    : ApiV1ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1BlogPost>>> List(CancellationToken ct)
    {
        var posts = await Catalog.Session.Query<V1BlogPostDocument>(collectionName: ApiV1Catalog.BlogPostsCollection).ToListAsync(ct);
        var result = new List<V1BlogPost>();
        foreach (var p in posts.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedAt)) // legacy: OrderByDescending(DateInsert)
            result.Add(await MapAsync(p, ct));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1BlogPost>> Get(int id, CancellationToken ct)
    {
        var post = await Catalog.LoadLiveAsync<V1BlogPostDocument>(ApiV1Catalog.BlogPostsCollection, id, ct);
        return post is null ? NotFound() : Ok(await MapAsync(post, ct));
    }

    private async Task<V1BlogPost> MapAsync(V1BlogPostDocument p, CancellationToken ct)
    {
        var author = p.AuthorId is null ? null : await Catalog.Session.LoadAsync<MintPlayerUser>(p.AuthorId, ct);
        return new V1BlogPost
        {
            Id = ApiV1Catalog.LegacyId(p),
            Title = p.Title,
            Headline = p.Headline,
            Body = p.Body,
            Author = author is null ? null : MapUser(author, sensitive: false),
            Published = Catalog.ToLegacyLocal(p.Published ?? p.CreatedAt),
        };
    }
}

[Route("api/v1/subject")]
public sealed class SubjectV1Controller
    : ApiV1ControllerBase
{
    private static readonly string[] SubjectCollections = ["Songs", "Artists", "People"];

    /// <summary>
    /// SubjectService.GetLikes: totals for any subject id (0/0 for an unknown id, like legacy) plus the
    /// caller's own like when signed in (JWT or cookie).
    /// </summary>
    [HttpGet("{subject_id:int}/likes")]
    public async Task<ActionResult<V1SubjectLikeResult>> Likes([FromRoute(Name = "subject_id")] int subjectId, CancellationToken ct)
    {
        var candidates = SubjectCollections.Select(c => $"{c}/{subjectId.ToString(CultureInfo.InvariantCulture)}").ToArray();
        var counts = await Catalog.Session.Query<LikeCount, Likes_Count>()
            .Where(x => x.SubjectId.In(candidates))
            .ToListAsync(ct);
        var hit = counts.FirstOrDefault();

        var user = await GetUserAsync();
        var result = new V1SubjectLikeResult
        {
            Likes = hit?.Likes ?? 0,
            Dislikes = hit?.Dislikes ?? 0,
            Authenticated = user is not null,
        };
        if (user is not null)
        {
            var doc = await Catalog.Session.LoadAsync<UserLike>($"UserLikes/{user.Id}", ct);
            if (doc is not null)
                result.Like = candidates.Any(c => doc.Likes.Contains(c, StringComparer.OrdinalIgnoreCase)) ? true
                            : candidates.Any(c => doc.Dislikes.Contains(c, StringComparer.OrdinalIgnoreCase)) ? false
                            : null;
        }
        return Ok(result);
    }
}
