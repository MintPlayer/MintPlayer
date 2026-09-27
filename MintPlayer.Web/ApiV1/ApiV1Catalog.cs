using System.Globalization;
using System.Text.RegularExpressions;
using MintPlayer.Domain.Entities;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace MintPlayer.Web.ApiV1;

/// <summary>
/// Read side of the legacy <c>api/v1</c> (spike S7): loads the live catalog from RavenDB once per request and
/// maps it onto the legacy DTO shapes (<see cref="V1Song"/> …), reproducing the legacy mappers
/// (<c>legacy/MintPlayer.Data/Mappers</c>) including their derived fields (<c>text</c>, <c>description</c>,
/// <c>youtubeId</c>, <c>playerInfos</c>, <c>dateUpdate</c>).
///
/// <para>The production catalog is ~1 200 documents, so every endpoint loads whole collections (one query
/// per collection, cached for the request) and resolves references and reverse references in memory. This
/// keeps the legacy EF include semantics easy to mirror; if the catalog grows by orders of magnitude, move the
/// reverse lookups (artist → songs, person → artists, tag → subjects) to static indexes.</para>
///
/// <para>Soft-deleted documents are invisible everywhere (legacy global query filter <c>UserDelete == null</c>).
/// Media whose type is hidden (<see cref="MediumType.Visible"/> false, D15) are shown only to
/// <see cref="Privileged"/> callers — legacy granted that to the Administrator role.</para>
/// </summary>
public sealed partial class ApiV1Catalog
{
    public const string BlogPostsCollection = "BlogPosts";

    private readonly IAsyncDocumentSession session;
    private readonly TimeZoneInfo legacyZone;

    private List<Artist>? artists;
    private List<Person>? people;
    private List<Song>? songs;
    private List<Tag>? tags;
    private List<TagCategory>? tagCategories;
    private Dictionary<string, MediumType>? mediumTypes;

    public ApiV1Catalog(IAsyncDocumentSession session, IConfiguration configuration)
    {
        this.session = session;
        legacyZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["ApiV1:LegacyTimeZone"] ?? "Europe/Amsterdam");
    }

    /// <summary>May see media of hidden medium types (legacy: Administrator).</summary>
    public bool Privileged { get; set; }

    public IAsyncDocumentSession Session => session;

    #region Loading

    private async Task<List<T>> LoadLiveAsync<T>(CancellationToken ct) where T : Entity
    {
        var all = await session.Query<T>().ToListAsync(ct);
        return all.Where(e => !e.IsDeleted).OrderBy(LegacyId).ToList();
    }

    public async Task<List<Artist>> ArtistsAsync(CancellationToken ct) => artists ??= await LoadLiveAsync<Artist>(ct);
    public async Task<List<Person>> PeopleAsync(CancellationToken ct) => people ??= await LoadLiveAsync<Person>(ct);
    public async Task<List<Song>> SongsAsync(CancellationToken ct) => songs ??= await LoadLiveAsync<Song>(ct);
    public async Task<List<Tag>> TagsAsync(CancellationToken ct) => tags ??= await LoadLiveAsync<Tag>(ct);
    public async Task<List<TagCategory>> TagCategoriesAsync(CancellationToken ct) => tagCategories ??= await LoadLiveAsync<TagCategory>(ct);

    /// <summary>Live medium types by id, hidden ones included (callers filter on <see cref="Privileged"/>).</summary>
    public async Task<Dictionary<string, MediumType>> MediumTypesAsync(CancellationToken ct)
        => mediumTypes ??= (await LoadLiveAsync<MediumType>(ct)).ToDictionary(m => m.Id!, StringComparer.OrdinalIgnoreCase);

    /// <summary>Everything the subject mappers dereference; call before mapping subjects.</summary>
    public async Task WarmAsync(CancellationToken ct)
    {
        await ArtistsAsync(ct);
        await PeopleAsync(ct);
        await SongsAsync(ct);
        await TagsAsync(ct);
        await TagCategoriesAsync(ct);
        await MediumTypesAsync(ct);
    }

    /// <summary>Loads a live entity by its legacy integer id (<c>{Collection}/{oldId}</c>, §5.2).</summary>
    public async Task<T?> LoadLiveAsync<T>(string collection, int legacyId, CancellationToken ct) where T : Entity
    {
        var entity = await session.LoadAsync<T>($"{collection}/{legacyId.ToString(CultureInfo.InvariantCulture)}", ct);
        return entity is { IsDeleted: false } ? entity : null;
    }

    #endregion

    #region Ids, dates, scalars

    /// <summary>
    /// The integer id legacy clients know: <see cref="Entity.OldId"/>, else the numeric suffix of a
    /// <c>{Collection}/{n}</c> id, else 0 (a document created after the cutover with a non-numeric id — see
    /// RESULT.md, open question 1).
    /// </summary>
    public static int LegacyId(Entity e)
    {
        if (e.OldId is int old) return old;
        var id = e.Id;
        var slash = id?.LastIndexOf('/') ?? -1;
        return slash >= 0 && int.TryParse(id.AsSpan(slash + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    public static int LegacyId(string? documentId)
    {
        var slash = documentId?.LastIndexOf('/') ?? -1;
        return slash >= 0 && int.TryParse(documentId.AsSpan(slash + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    /// <summary>Legacy stored local wall-clock times (<c>DateTime.Now</c>); the migration made them UTC.</summary>
    public DateTime ToLegacyLocal(DateTimeOffset instant)
        => DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(instant, legacyZone).DateTime, DateTimeKind.Unspecified);

    private DateTime DateUpdate(Entity e) => ToLegacyLocal(e.ModifiedAt ?? e.CreatedAt);

    private static DateTime? ToDateTime(DateOnly? d) => d?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

    private string? ChangeVector(object entity)
    {
        try { return session.Advanced.GetChangeVectorFor(entity); }
        catch (InvalidOperationException) { return null; }
    }

    /// <summary><c>ColorTranslator.ToHtml</c> of an ARGB colour (legacy colours are never "known" colours).</summary>
    public static string ToHtml(System.Drawing.Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    #endregion

    #region Medium types, media, player infos

    public V1MediumType MapMediumType(MediumType t) => new()
    {
        Id = LegacyId(t),
        Description = t.Name,
        Visible = t.Visible,
    };

    /// <summary>Media shown to this caller: live type, and visible unless <see cref="Privileged"/> (D15).</summary>
    private IEnumerable<(Medium Medium, MediumType Type)> ShownMedia(Subject s)
    {
        foreach (var m in s.Media)
        {
            if (m.TypeId is null || !mediumTypes!.TryGetValue(m.TypeId, out var type)) continue;
            if (!type.Visible && !Privileged) continue;
            yield return (m, type);
        }
    }

    private List<V1Medium> MapMedia(Subject s)
        => ShownMedia(s).Select(x => new V1Medium { Id = 0, Type = MapMediumType(x.Type), Value = x.Medium.Value }).ToList();

    // legacy/MintPlayer.Data/Helpers/SongHelper.GetPlayerInfos — same regexes, same order.
    private static readonly (Regex Regex, int Type)[] PlayableRegexes =
    [
        (YoutubeWatch(), 1),
        (YoutubeMobile(), 1),
        (YoutubeShort(), 1),
        (Dailymotion(), 2),
        (Vimeo(), 3),
        (SoundCloud(), 4),
    ];

    [GeneratedRegex(@"http[s]{0,1}:\/\/(www\.){0,1}youtube\.com\/watch\?v=(?<id>[^&]+)")] private static partial Regex YoutubeWatch();
    [GeneratedRegex(@"http[s]{0,1}:\/\/m\.youtube\.com\/watch\?v=(?<id>[^&]+)")] private static partial Regex YoutubeMobile();
    [GeneratedRegex(@"http[s]{0,1}:\/\/(www\.){0,1}youtu\.be\/(?<id>.+)$")] private static partial Regex YoutubeShort();
    [GeneratedRegex(@"http[s]{0,1}:\/\/(www\.){0,1}dailymotion\.com\/video\/(?<id>[0-9A-Za-z]+)$")] private static partial Regex Dailymotion();
    [GeneratedRegex(@"http[s]{0,1}:\/\/(www\.){0,1}vimeo\.com\/(?<id>[0-9]+)$")] private static partial Regex Vimeo();
    [GeneratedRegex(@"(?<id>http[s]{0,1}:\/\/(www\.){0,1}soundcloud\.com\/.+)$")] private static partial Regex SoundCloud();

    private List<V1PlayerInfo> PlayerInfos(Subject s)
    {
        var result = new List<V1PlayerInfo>();
        foreach (var (medium, _) in ShownMedia(s))
        {
            foreach (var (regex, type) in PlayableRegexes)
            {
                var match = regex.Match(medium.Value);
                if (!match.Success) continue;
                var id = match.Groups["id"].Value;
                result.Add(new V1PlayerInfo
                {
                    Type = type,
                    Id = id,
                    Url = medium.Value,
                    ImageUrl = type switch
                    {
                        1 => $"https://i.ytimg.com/vi/{id}/hqdefault.jpg",
                        2 => $"https://www.dailymotion.com/thumbnail/video/{id}",
                        3 => "https://i.vimeocdn.com/video/99213072?mw=960&mh=540",
                        4 => "https://i1.sndcdn.com/artworks-eA5afLkRFfiD-0-t500x500.jpg",
                        _ => null,
                    },
                });
            }
        }
        return result;
    }

    #endregion

    #region Tags

    /// <summary>The live tags (after <see cref="TagsAsync"/>).</summary>
    public List<Tag> LoadedTags => tags ?? throw new InvalidOperationException("Tags are not loaded.");

    public TagCategory? FindCategory(string? id) => id is null ? null : tagCategories!.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
    public Tag? FindTag(string? id) => id is null ? null : tags!.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>TagCategoryMapper.Entity2Dto without relations.</summary>
    public V1TagCategory MapCategory(TagCategory c) => new()
    {
        Id = LegacyId(c),
        Color = ToHtml(c.Color),
        Description = c.Description,
    };

    /// <summary>
    /// TagMapper.Entity2Dto(tag) without subjects/parent/children. <paramref name="categoryShown"/> mirrors
    /// whether the legacy EF query had the category entity loaded (included or tracked by fix-up).
    /// </summary>
    public V1Tag MapTagShallow(Tag t, Func<string, bool> categoryShown)
    {
        var category = t.CategoryId is not null && categoryShown(t.CategoryId) ? FindCategory(t.CategoryId) : null;
        return new V1Tag
        {
            Id = LegacyId(t),
            Description = t.Description,
            Category = category is null ? null : MapCategory(category),
        };
    }

    private List<V1Tag> MapSubjectTags(Subject s, bool withCategory)
        => s.TagIds.Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(FindTag).OfType<Tag>()
            .OrderBy(LegacyId)
            .Select(t => MapTagShallow(t, _ => withCategory))
            .ToList();

    #endregion

    #region Subjects

    public Artist? FindArtist(string? id) => id is null ? null : artists!.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase));
    public Person? FindPerson(string? id) => id is null ? null : people!.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
    public Song? FindSong(string? id) => id is null ? null : songs!.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

    private void FillSubject(V1Subject dto, Subject s, string text)
    {
        dto.Id = LegacyId(s);
        dto.Text = text;
        dto.ConcurrencyStamp = ChangeVector(s);
        dto.DateUpdate = DateUpdate(s);
    }

    /// <summary>Which legacy relations of a song were loaded by the EF query of the endpoint being mirrored.</summary>
    public sealed record SongLoad(bool Relations, bool Media, bool TagCategories, Func<Song, IEnumerable<Artist>>? DescriptionArtists)
    {
        /// <summary>Song list / song by id: media + artists (+ lyrics, tags when related).</summary>
        public static SongLoad Full(bool relations, bool tagCategories) => new(relations, true, tagCategories, null);
        /// <summary>A nested song whose media and artists the legacy query did not load.</summary>
        public static readonly SongLoad Bare = new(false, false, false, _ => []);
        /// <summary>A nested song with media loaded but no artists (playlist list tracks).</summary>
        public static readonly SongLoad MediaOnly = new(false, true, false, _ => []);
    }

    /// <summary>The song's live artists in legacy (ArtistSong key) order.</summary>
    public IEnumerable<(Artist Artist, bool Credited)> SongArtists(Song s)
        => s.Artists
            .Select(a => (Artist: FindArtist(a.ArtistId), a.Credited))
            .Where(x => x.Artist is not null)
            .Select(x => (x.Artist!, x.Credited))
            .OrderBy(x => LegacyId(x.Item1));

    public V1Song MapSong(Song s, SongLoad load)
    {
        var descriptionArtists = (load.DescriptionArtists?.Invoke(s) ?? SongArtists(s).Select(x => x.Artist)).ToList();
        var playerInfos = load.Media ? PlayerInfos(s) : [];
        var dto = new V1Song
        {
            Title = s.Title,
            Released = ToDateTime(s.Released) ?? default,
            Description = descriptionArtists.Count > 0 ? $"{s.Title} - {string.Join(" & ", descriptionArtists.Select(a => a.Name))}" : s.Title,
            YoutubeId = playerInfos.FirstOrDefault(p => p.Type == 1)?.Id,
            DailymotionId = playerInfos.FirstOrDefault(p => p.Type == 2)?.Id,
            VimeoId = playerInfos.FirstOrDefault(p => p.Type == 3)?.Id,
            SoundCloudUrl = playerInfos.FirstOrDefault(p => p.Type == 4)?.Id,
            PlayerInfos = playerInfos,
        };
        FillSubject(dto, s, s.Title);
        if (load.Relations)
        {
            dto.Lyrics = MapLyrics(s);
            var songArtists = SongArtists(s).ToList();
            dto.Artists = songArtists.Where(x => x.Credited).Select(x => MapArtistShallow(x.Artist)).ToList();
            dto.UncreditedArtists = songArtists.Where(x => !x.Credited).Select(x => MapArtistShallow(x.Artist)).ToList();
            dto.Media = MapMedia(s);
            dto.Tags = MapSubjectTags(s, load.TagCategories);
        }
        return dto;
    }

    /// <summary>
    /// Legacy <c>Lyrics { Text, Timeline }</c>: the timeline had one entry (seconds) per non-empty line. The new
    /// model keeps one start time per line (blank lines and unsynced lines = null) per medium (S6), so the
    /// legacy timeline is the non-null starts of the non-empty lines of the (first) timing.
    /// </summary>
    public static V1Lyrics MapLyrics(Song s)
    {
        var text = s.Lyrics ?? string.Empty;
        var timeline = new List<double>();
        var timing = s.LyricsTimings.FirstOrDefault();
        if (timing is not null)
        {
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length && i < timing.StartTimes.Count; i++)
            {
                if (lines[i].Length == 0) continue;
                if (timing.StartTimes[i] is double start) timeline.Add(start);
            }
        }
        return new V1Lyrics { Text = text, Timeline = timeline };
    }

    public V1Artist MapArtistShallow(Artist a)
    {
        var dto = new V1Artist { Name = a.Name, YearStarted = a.YearStarted, YearQuit = a.YearQuit };
        FillSubject(dto, a, a.Name);
        return dto;
    }

    /// <summary>
    /// ArtistMapper.Entity2Dto with relations. <paramref name="trackedArtistIds"/> mirrors EF relationship
    /// fix-up: a nested song's <c>description</c> lists only the artists whose ArtistSong rows the legacy query
    /// had tracked by then (see RESULT.md delta D-3).
    /// </summary>
    public V1Artist MapArtist(Artist a, bool relations, bool tagCategories, ISet<string>? trackedArtistIds = null)
    {
        var dto = MapArtistShallow(a);
        if (!relations) return dto;

        var members = a.Members
            .Select(m => (Person: FindPerson(m.PersonId), m.Active))
            .Where(x => x.Person is not null)
            .OrderBy(x => LegacyId(x.Person!))
            .ToList();
        dto.PastMembers = members.Where(x => !x.Active).Select(x => MapPersonShallow(x.Person!)).ToList();
        dto.CurrentMembers = members.Where(x => x.Active).Select(x => MapPersonShallow(x.Person!)).ToList();

        var tracked = trackedArtistIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        tracked.Add(a.Id!);
        var bare = SongLoad.Bare with
        {
            DescriptionArtists = song => SongArtists(song).Select(x => x.Artist).Where(x => tracked.Contains(x.Id!)),
        };
        dto.Songs = songs!
            .Where(s => s.Artists.Any(x => string.Equals(x.ArtistId, a.Id, StringComparison.OrdinalIgnoreCase)))
            .Select(s => MapSong(s, bare))
            .ToList();
        dto.Media = MapMedia(a);
        dto.Tags = MapSubjectTags(a, tagCategories);
        return dto;
    }

    public V1Person MapPersonShallow(Person p)
    {
        var dto = new V1Person
        {
            FirstName = p.FirstName,
            LastName = p.LastName,
            Born = ToDateTime(p.Born),
            Died = ToDateTime(p.Died),
        };
        FillSubject(dto, p, $"{p.FirstName} {p.LastName}");
        return dto;
    }

    public V1Person MapPerson(Person p, bool relations, bool tagCategories)
    {
        var dto = MapPersonShallow(p);
        if (!relations) return dto;
        dto.Artists = artists!
            .Where(a => a.Members.Any(m => string.Equals(m.PersonId, p.Id, StringComparison.OrdinalIgnoreCase)))
            .Select(MapArtistShallow)
            .ToList();
        dto.Media = MapMedia(p);
        dto.Tags = MapSubjectTags(p, tagCategories);
        return dto;
    }

    /// <summary>SubjectMapper.Entity2Dto(subject, include_relations: false) — a tag's subjects.</summary>
    public object MapSubjectShallow(Subject s) => s switch
    {
        Song song => MapSong(song, SongLoad.Bare),
        Artist artist => MapArtistShallow(artist),
        Person person => MapPersonShallow(person),
        _ => throw new ArgumentException($"Unknown subject type {s.GetType().Name}", nameof(s)),
    };

    /// <summary>Live subjects carrying the tag, in legacy id order (subject ids are one sequence in legacy).</summary>
    public IEnumerable<Subject> SubjectsWithTag(Tag t)
        => artists!.Cast<Subject>().Concat(people!).Concat(songs!)
            .Where(s => s.TagIds.Contains(t.Id!, StringComparer.OrdinalIgnoreCase))
            .OrderBy(LegacyId);

    #endregion
}
