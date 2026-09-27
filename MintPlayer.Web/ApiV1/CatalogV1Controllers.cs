using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MintPlayer.Domain.Entities;

namespace MintPlayer.Web.ApiV1;

// Catalog read endpoints of the legacy public API (legacy/MintPlayer.Web/Server/Controllers/Api/V1). Each
// action names the legacy EF query it mirrors, because what that query loaded decides which nested fields
// are null / empty in the legacy response.

[Route("api/v1/artist")]
public sealed class ArtistV1Controller
    : ApiV1ControllerBase
{
    /// <summary>ArtistRepository.GetArtists.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1Artist>>> List(CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var rel = IncludeRelations;
        var tracked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return Ok((await Catalog.ArtistsAsync(ct)).Select(a => Catalog.MapArtist(a, rel, tagCategories: false, tracked)).ToList());
    }

    /// <summary>ArtistRepository.GetArtist (tags include their category).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1Artist>> Get(int id, CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var artist = Catalog.FindArtist($"Artists/{id.ToString(CultureInfo.InvariantCulture)}");
        return artist is null ? NotFound() : Ok(Catalog.MapArtist(artist, IncludeRelations, tagCategories: true));
    }
}

[Route("api/v1/person")]
public sealed class PersonV1Controller
    : ApiV1ControllerBase
{
    /// <summary>PersonRepository.GetPeople.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1Person>>> List(CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var rel = IncludeRelations;
        return Ok((await Catalog.PeopleAsync(ct)).Select(p => Catalog.MapPerson(p, rel, tagCategories: false)).ToList());
    }

    /// <summary>PersonRepository.GetPerson.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1Person>> Get(int id, CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var person = Catalog.FindPerson($"People/{id.ToString(CultureInfo.InvariantCulture)}");
        return person is null ? NotFound() : Ok(Catalog.MapPerson(person, IncludeRelations, tagCategories: true));
    }
}

[Route("api/v1/song")]
public sealed class SongV1Controller
    : ApiV1ControllerBase
{
    /// <summary>SongRepository.GetSongs (media + artists always loaded; lyrics + tags with relations, tags without category).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1Song>>> List(CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var load = ApiV1Catalog.SongLoad.Full(IncludeRelations, tagCategories: false);
        return Ok((await Catalog.SongsAsync(ct)).Select(s => Catalog.MapSong(s, load)).ToList());
    }

    /// <summary>SongRepository.GetSong.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1Song>> Get(int id, CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var song = Catalog.FindSong($"Songs/{id.ToString(CultureInfo.InvariantCulture)}");
        return song is null ? NotFound() : Ok(Catalog.MapSong(song, ApiV1Catalog.SongLoad.Full(IncludeRelations, tagCategories: true)));
    }

    /// <summary>
    /// SongController.Lyrics. Legacy dereferenced a missing song (500); this returns 404.
    /// </summary>
    [HttpGet("{id:int}/lyrics")]
    public async Task<ActionResult<V1Lyrics>> Lyrics(int id, CancellationToken ct)
    {
        var song = await Catalog.LoadLiveAsync<Song>("Songs", id, ct);
        return song is null ? NotFound() : Ok(ApiV1Catalog.MapLyrics(song));
    }

    /// <summary>
    /// SongRepository.PageSongs: sorts on an entity property, pages, and maps without media or artists
    /// (so <c>playerInfos</c> is empty and <c>description</c> is the title — legacy behaviour).
    /// </summary>
    [HttpPost("page")]
    public async Task<ActionResult<V1PaginationResponse<V1Song>>> Page([FromBody] V1PaginationRequest request, CancellationToken ct)
    {
        if (request.PerPage <= 0 || request.Page <= 0) return BadRequest("page and perPage must be positive.");
        Func<Song, object?>? key = (request.SortProperty ?? "Id").ToLowerInvariant() switch
        {
            "id" => s => ApiV1Catalog.LegacyId(s),
            "title" or "text" or "description" => s => s.Title,
            "released" => s => s.Released,
            "dateupdate" => s => s.ModifiedAt ?? s.CreatedAt,
            _ => null,
        };
        if (key is null) return BadRequest($"Unknown sort property '{request.SortProperty}'.");

        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var all = await Catalog.SongsAsync(ct);
        // SQL Server's default collation compares case-insensitively; InvariantCultureIgnoreCase is the closest match.
        var comparer = Comparer<object?>.Create((x, y) => x is string a && y is string b
            ? StringComparer.InvariantCultureIgnoreCase.Compare(a, b)
            : Comparer<object?>.Default.Compare(x, y));
        var ordered = request.SortDirection == 1 ? all.OrderByDescending(key, comparer) : all.OrderBy(key, comparer);
        var data = ordered.Skip((request.Page - 1) * request.PerPage).Take(request.PerPage)
            .Select(s => Catalog.MapSong(s, ApiV1Catalog.SongLoad.Bare with { DescriptionArtists = _ => [] }))
            .ToList();
        return Ok(new V1PaginationResponse<V1Song>
        {
            Data = data,
            Page = request.Page,
            PerPage = request.PerPage,
            TotalRecords = all.Count,
            TotalPages = (int)Math.Ceiling(all.Count / (double)request.PerPage),
        });
    }
}

[Route("api/v1/mediumtype")]
public sealed class MediumTypeV1Controller
    : ApiV1ControllerBase
{
    /// <summary>MediumTypeRepository.GetMediumTypes — hidden types only for privileged callers.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1MediumType>>> List(CancellationToken ct)
    {
        await PrepareAsync(ct);
        var types = (await Catalog.MediumTypesAsync(ct)).Values
            .Where(t => t.Visible || Catalog.Privileged)
            .OrderBy(ApiV1Catalog.LegacyId)
            .Select(Catalog.MapMediumType)
            .ToList();
        return Ok(types);
    }

    /// <summary>MediumTypeRepository.GetMediumType — a hidden type is 404 for non-privileged callers.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1MediumType>> Get(int id, CancellationToken ct)
    {
        await PrepareAsync(ct);
        var types = await Catalog.MediumTypesAsync(ct);
        return types.TryGetValue($"MediumTypes/{id.ToString(CultureInfo.InvariantCulture)}", out var t) && (t.Visible || Catalog.Privileged)
            ? Ok(Catalog.MapMediumType(t))
            : NotFound();
    }
}

[Route("api/v1/tag")]
public sealed class TagV1Controller
    : ApiV1ControllerBase
{
    /// <summary>TagRepository.GetTags (<c>root_tags_only</c> header, default true).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1Tag>>> List(CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var rootOnly = HeaderFlag("root_tags_only", true);
        var rel = IncludeRelations;
        var all = await Catalog.TagsAsync(ct);
        var selected = all.Where(t => !rootOnly || Catalog.FindTag(t.ParentId) is null).ToList();
        // EF fix-up: a category is on a tag when some tag of the result had it included.
        var tracked = new HashSet<string>(selected.Where(t => t.CategoryId is not null).Select(t => t.CategoryId!), StringComparer.OrdinalIgnoreCase);
        return Ok(selected.Select(t => MapTag(t, rel, tracked)).ToList());
    }

    /// <summary>TagRepository.GetTag.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1Tag>> Get(int id, CancellationToken ct)
    {
        await PrepareAsync(ct);
        await Catalog.WarmAsync(ct);
        var tag = Catalog.FindTag($"Tags/{id.ToString(CultureInfo.InvariantCulture)}");
        if (tag is null) return NotFound();
        var tracked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (tag.CategoryId is not null) tracked.Add(tag.CategoryId);
        return Ok(MapTag(tag, IncludeRelations, tracked));
    }

    private V1Tag MapTag(Tag t, bool rel, HashSet<string> trackedCategories)
    {
        var dto = Catalog.MapTagShallow(t, trackedCategories.Contains);
        if (!rel) return dto;
        dto.Subjects = Catalog.SubjectsWithTag(t).Select(Catalog.MapSubjectShallow).ToList();
        var parent = Catalog.FindTag(t.ParentId);
        dto.Parent = parent is null ? null : Catalog.MapTagShallow(parent, trackedCategories.Contains);
        dto.Children = Catalog.LoadedTags
            .Where(c => string.Equals(c.ParentId, t.Id, StringComparison.OrdinalIgnoreCase))
            .Select(c => Catalog.MapTagShallow(c, trackedCategories.Contains))
            .ToList();
        return dto;
    }
}

[Route("api/v1/tagcategory")]
public sealed class TagCategoryV1Controller
    : ApiV1ControllerBase
{
    /// <summary>TagCategoryRepository.GetTagCategories.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<V1TagCategory>>> List(CancellationToken ct)
    {
        var categories = await Catalog.TagCategoriesAsync(ct);
        var tags = await Catalog.TagsAsync(ct);
        var rel = IncludeRelations;
        // With relations every live category's tags are tracked, so a tag's Parent is "loaded" when the parent
        // belongs to a live category.
        var loaded = rel
            ? new HashSet<string>(tags.Where(t => Catalog.FindCategory(t.CategoryId) is not null).Select(t => t.Id!), StringComparer.OrdinalIgnoreCase)
            : [];
        return Ok(categories.Select(c => Map(c, rel, tags, loaded)).ToList());
    }

    /// <summary>TagCategoryRepository.GetTagCategory.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<V1TagCategory>> Get(int id, CancellationToken ct)
    {
        var tags = await Catalog.TagsAsync(ct);
        await Catalog.TagCategoriesAsync(ct);
        var category = Catalog.FindCategory($"TagCategories/{id.ToString(CultureInfo.InvariantCulture)}");
        if (category is null) return NotFound();
        var loaded = new HashSet<string>(tags.Where(t => string.Equals(t.CategoryId, category.Id, StringComparison.OrdinalIgnoreCase)).Select(t => t.Id!), StringComparer.OrdinalIgnoreCase);
        return Ok(Map(category, IncludeRelations, tags, loaded));
    }

    /// <summary>
    /// TagCategoryMapper.Entity2Dto(tc, true): the category's tags whose Parent navigation is null in memory,
    /// i.e. root tags plus tags whose parent was not loaded by the query.
    /// </summary>
    private V1TagCategory Map(TagCategory c, bool rel, List<Tag> tags, HashSet<string> loaded)
    {
        var dto = Catalog.MapCategory(c);
        if (!rel) return dto;
        dto.Tags = tags
            .Where(t => string.Equals(t.CategoryId, c.Id, StringComparison.OrdinalIgnoreCase))
            .Where(t => t.ParentId is null || !loaded.Contains(t.ParentId))
            .Select(t => Catalog.MapTagShallow(t, _ => true))
            .ToList();
        return dto;
    }
}
