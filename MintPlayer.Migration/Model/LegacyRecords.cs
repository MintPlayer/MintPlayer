namespace MintPlayer.Migration.Model;

// Neutral records mirroring the production schema as it exists in the snapshot (last migration
// 20240627124318_xxx, schema `mintplay`) — NOT the repo's latest EF ModelSnapshot. Plain data, no EF:
// the legacy EF context applies query filters and value converters (Timeline x20, Color) that would
// hide or rewrite exactly the data the migration has to see raw. Legacy DateTime values are local
// Europe/Amsterdam wall-clock times (D19) and stay unconverted here; Transform converts them.

public sealed record LegacySubject(
    int Id,
    string SubjectType,
    Guid? UserDeleteId,
    DateTime DateInsert,
    DateTime? DateUpdate,
    DateTime? DateDelete,
    string? Name,
    int? YearStarted,
    int? YearQuit,
    string? FirstName,
    string? LastName,
    DateTime? Born,
    DateTime? Died,
    string? Title,
    DateTime? Released);

public sealed record LegacyMedium(int Id, int TypeId, int SubjectId, string Value);

public sealed record LegacyMediumType(int Id, string Description, Guid UserDeleteId, bool Visible);

public sealed record LegacyTag(
    int Id, string Description, int CategoryId, int ParentId,
    Guid UserDeleteId, DateTime DateInsert, DateTime? DateUpdate, DateTime? DateDelete);

public sealed record LegacyTagCategory(
    int Id, int Color, string Description,
    Guid UserDeleteId, DateTime DateInsert, DateTime? DateUpdate, DateTime? DateDelete);

public sealed record LegacySubjectTag(int SubjectId, int TagId);

public sealed record LegacyArtistSong(int ArtistId, int SongId, bool Credited);

public sealed record LegacyArtistPerson(int ArtistId, int PersonId, bool Active);

public sealed record LegacyLyrics(int SongId, Guid UserId, string Text, DateTime UpdatedAt, string? Timeline);

public sealed record LegacyLike(int SubjectId, Guid UserId, bool DoesLike);

public sealed record LegacyPlaylist(int Id, Guid UserId, string Description, bool IsDeleted, int Accessibility);

public sealed record LegacyPlaylistSong(int PlaylistId, int SongId, int Index);

public sealed record LegacyBlogPost(
    int Id, string Title, string Headline, string Body,
    Guid UserInsertId, Guid UserDeleteId, DateTime DateInsert, DateTime? DateUpdate, DateTime? DateDelete);

public sealed record LegacyUser(
    Guid Id,
    string? UserName,
    string? NormalizedUserName,
    string? Email,
    string? NormalizedEmail,
    bool EmailConfirmed,
    string? PasswordHash,
    string? SecurityStamp,
    string? ConcurrencyStamp,
    string? PhoneNumber,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled,
    DateTimeOffset? LockoutEnd,
    bool LockoutEnabled,
    int AccessFailedCount,
    string? PictureUrl,
    bool Bypass2faForExternalLogin);

public sealed record LegacyRole(Guid Id, string? Name);

public sealed record LegacyUserRole(Guid UserId, Guid RoleId);

public sealed record LegacyUserLogin(Guid UserId, string LoginProvider, string ProviderKey, string? ProviderDisplayName);

public sealed record LegacyUserClaim(Guid UserId, string? ClaimType, string? ClaimValue);

public sealed record LegacyUserToken(Guid UserId, string LoginProvider, string Name, string? Value);

/// <summary>The whole legacy database, materialised. Production is ~10 MB, so this is cheap and lets
/// every transform be a pure function over in-memory data.</summary>
public sealed class LegacySnapshot
{
    public List<LegacySubject> Subjects { get; init; } = [];
    public List<LegacyMedium> Media { get; init; } = [];
    public List<LegacyMediumType> MediumTypes { get; init; } = [];
    public List<LegacyTag> Tags { get; init; } = [];
    public List<LegacyTagCategory> TagCategories { get; init; } = [];
    public List<LegacySubjectTag> SubjectTags { get; init; } = [];
    public List<LegacyArtistSong> ArtistSongs { get; init; } = [];
    public List<LegacyArtistPerson> ArtistPersons { get; init; } = [];
    public List<LegacyLyrics> Lyrics { get; init; } = [];
    public List<LegacyLike> Likes { get; init; } = [];
    public List<LegacyPlaylist> Playlists { get; init; } = [];
    public List<LegacyPlaylistSong> PlaylistSongs { get; init; } = [];
    public List<LegacyBlogPost> BlogPosts { get; init; } = [];
    public List<LegacyUser> Users { get; init; } = [];
    public List<LegacyRole> Roles { get; init; } = [];
    public List<LegacyUserRole> UserRoles { get; init; } = [];
    public List<LegacyUserLogin> UserLogins { get; init; } = [];
    public List<LegacyUserClaim> UserClaims { get; init; } = [];
    public List<LegacyUserToken> UserTokens { get; init; } = [];

    /// <summary>Row counts of the tables that are deliberately not migrated (reported, not read).</summary>
    public Dictionary<string, int> DroppedTableCounts { get; init; } = [];
}
