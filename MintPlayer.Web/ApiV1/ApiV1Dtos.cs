namespace MintPlayer.Web.ApiV1;

// Wire shapes of the legacy public API (legacy/MintPlayer.Dtos), reproduced field for field so existing
// api/v1 consumers keep working. Property names serialize camelCase through the default System.Text.Json
// web options, which matches the legacy Newtonsoft output (nulls included). Do not rename or drop members:
// the Swagger document at https://mintplayer.com/swagger/v1/swagger.json is the contract (spike S7).

public abstract class V1Subject
{
    public int Id { get; set; }
    public List<V1Medium>? Media { get; set; }
    public List<V1Tag>? Tags { get; set; }
    public string? Text { get; set; }
    /// <summary>Legacy: base64 SQL rowversion. Now the RavenDB change vector (opaque; see RESULT.md).</summary>
    public string? ConcurrencyStamp { get; set; }
    /// <summary>Legacy <c>DateUpdate ?? DateInsert</c>, a local (Europe/Amsterdam) time without offset.</summary>
    public DateTime DateUpdate { get; set; }
}

public sealed class V1Artist : V1Subject
{
    public string? Name { get; set; }
    public int? YearStarted { get; set; }
    public int? YearQuit { get; set; }
    public List<V1Person>? PastMembers { get; set; }
    public List<V1Person>? CurrentMembers { get; set; }
    public List<V1Song>? Songs { get; set; }
}

public sealed class V1Person : V1Subject
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Born { get; set; }
    public DateTime? Died { get; set; }
    public List<V1Artist>? Artists { get; set; }
}

public sealed class V1Song : V1Subject
{
    public string? Title { get; set; }
    public DateTime Released { get; set; }
    public V1Lyrics? Lyrics { get; set; }
    public string? YoutubeId { get; set; }
    public string? DailymotionId { get; set; }
    public string? VimeoId { get; set; }
    public string? SoundCloudUrl { get; set; }
    public List<V1PlayerInfo>? PlayerInfos { get; set; }
    public string? Description { get; set; }
    public List<V1Artist>? Artists { get; set; }
    public List<V1Artist>? UncreditedArtists { get; set; }
}

public sealed class V1Lyrics
{
    public string? Text { get; set; }
    public List<double>? Timeline { get; set; }
}

public sealed class V1PlayerInfo
{
    /// <summary><c>ePlayerType</c>: 0 None, 1 Youtube, 2 DailyMotion, 3 Vimeo, 4 SoundCloud.</summary>
    public int Type { get; set; }
    public string? Id { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
}

public sealed class V1Medium
{
    /// <summary>Legacy <c>Media.Id</c> — not migrated (media are embedded now); always 0.</summary>
    public int Id { get; set; }
    public V1MediumType? Type { get; set; }
    public string? Value { get; set; }
}

public sealed class V1MediumType
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public bool Visible { get; set; }
}

public sealed class V1Tag
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public V1TagCategory? Category { get; set; }
    /// <summary>Polymorphic: <see cref="V1Artist"/>, <see cref="V1Person"/> or <see cref="V1Song"/>.</summary>
    public List<object>? Subjects { get; set; }
    public V1Tag? Parent { get; set; }
    public List<V1Tag>? Children { get; set; }
    public string? Text => Description;
}

public sealed class V1TagCategory
{
    public int Id { get; set; }
    /// <summary>HTML colour (<c>ColorTranslator.ToHtml</c>): <c>#RRGGBB</c>.</summary>
    public string? Color { get; set; }
    public string? Description { get; set; }
    public List<V1Tag>? Tags { get; set; }
}

public sealed class V1Playlist
{
    public int Id { get; set; }
    public V1User? User { get; set; }
    public string? Description { get; set; }
    public List<V1Song>? Tracks { get; set; }
    /// <summary><c>ePlaylistAccessibility</c>: 0 Private, 1 Public.</summary>
    public int Accessibility { get; set; }
}

public sealed class V1User
{
    /// <summary>A GUID for migrated users (<c>MintPlayerUsers/{guid}</c>); zero GUID when not "sensitive".</summary>
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool Bypass2faForExternalLogin { get; set; }
    public string? PictureUrl { get; set; }
}

public sealed class V1BlogPost
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Body { get; set; }
    public V1User? Author { get; set; }
    public DateTime Published { get; set; }
}

public sealed class V1SubjectLikeResult
{
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public bool? Like { get; set; }
    public bool Authenticated { get; set; }
}

public sealed class V1LoginRequest
{
    /// <summary>Email address, or (unlike legacy) the user name.</summary>
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public sealed class V1LocalLoginResult
{
    /// <summary><c>LoginStatus</c>: 0 Failed, 1 Success, 2 RequiresTwoFactor.</summary>
    public int Status { get; set; }
    public V1User? User { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
    public string? Token { get; set; }
}

public sealed class V1SortColumn
{
    public string? Property { get; set; }
    public int Direction { get; set; }
}

public sealed class V1PaginationRequest
{
    public int PerPage { get; set; }
    public int Page { get; set; }
    public List<V1SortColumn>? SortColumns { get; set; }
    public string? SortProperty { get; set; }
    /// <summary><c>ListSortDirection</c>: 0 Ascending, 1 Descending.</summary>
    public int SortDirection { get; set; }
}

public sealed class V1PaginationResponse<T>
{
    public List<T> Data { get; set; } = [];
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
