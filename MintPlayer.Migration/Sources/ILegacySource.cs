using MintPlayer.Migration.Model;

namespace MintPlayer.Migration.Sources;

[Flags]
public enum SourceCapabilities
{
    None = 0,
    /// <summary>Artists/People/Songs/Media/Tags/MediumTypes/Lyrics.</summary>
    Catalog = 1,
    /// <summary>Rows the public API hides: soft-deleted rows, audit columns, hidden medium types.</summary>
    DeletedAndAudit = 2,
    /// <summary>AspNetUsers + roles/logins/claims/tokens, likes, private playlists.</summary>
    Identity = 4,
    BlogPosts = 8,
    All = Catalog | DeletedAndAudit | Identity | BlogPosts,
}

/// <summary>
/// A source of legacy rows, one stream per table. <see cref="SqlSnapshotSource"/> (a restored .bak) is
/// the complete, primary source (D9). A public-API source would only have <see cref="SourceCapabilities.Catalog"/>.
/// </summary>
public interface ILegacySource
{
    SourceCapabilities Capabilities { get; }
    IAsyncEnumerable<LegacySubject> Subjects(CancellationToken ct = default);
    IAsyncEnumerable<LegacyMedium> Media(CancellationToken ct = default);
    IAsyncEnumerable<LegacyMediumType> MediumTypes(CancellationToken ct = default);
    IAsyncEnumerable<LegacyTag> Tags(CancellationToken ct = default);
    IAsyncEnumerable<LegacyTagCategory> TagCategories(CancellationToken ct = default);
    IAsyncEnumerable<LegacySubjectTag> SubjectTags(CancellationToken ct = default);
    IAsyncEnumerable<LegacyArtistSong> ArtistSongs(CancellationToken ct = default);
    IAsyncEnumerable<LegacyArtistPerson> ArtistPersons(CancellationToken ct = default);
    IAsyncEnumerable<LegacyLyrics> Lyrics(CancellationToken ct = default);
    IAsyncEnumerable<LegacyLike> Likes(CancellationToken ct = default);
    IAsyncEnumerable<LegacyPlaylist> Playlists(CancellationToken ct = default);
    IAsyncEnumerable<LegacyPlaylistSong> PlaylistSongs(CancellationToken ct = default);
    IAsyncEnumerable<LegacyBlogPost> BlogPosts(CancellationToken ct = default);
    IAsyncEnumerable<LegacyUser> Users(CancellationToken ct = default);
    IAsyncEnumerable<LegacyRole> Roles(CancellationToken ct = default);
    IAsyncEnumerable<LegacyUserRole> UserRoles(CancellationToken ct = default);
    IAsyncEnumerable<LegacyUserLogin> UserLogins(CancellationToken ct = default);
    IAsyncEnumerable<LegacyUserClaim> UserClaims(CancellationToken ct = default);
    IAsyncEnumerable<LegacyUserToken> UserTokens(CancellationToken ct = default);
    /// <summary>Row counts of tables that are dropped by design (Jobs, LogEntries).</summary>
    Task<Dictionary<string, int>> DroppedTableCountsAsync(CancellationToken ct = default);
}

public static class LegacySourceExtensions
{
    /// <summary>Materialises every table of the source into one in-memory snapshot.</summary>
    public static async Task<LegacySnapshot> LoadSnapshotAsync(this ILegacySource source, CancellationToken ct = default)
    {
        if (!source.Capabilities.HasFlag(SourceCapabilities.All))
            throw new NotSupportedException($"Source {source.GetType().Name} is not complete ({source.Capabilities}); a full migration needs {SourceCapabilities.All}.");

        return new LegacySnapshot
        {
            Subjects = await ToList(source.Subjects(ct), ct),
            Media = await ToList(source.Media(ct), ct),
            MediumTypes = await ToList(source.MediumTypes(ct), ct),
            Tags = await ToList(source.Tags(ct), ct),
            TagCategories = await ToList(source.TagCategories(ct), ct),
            SubjectTags = await ToList(source.SubjectTags(ct), ct),
            ArtistSongs = await ToList(source.ArtistSongs(ct), ct),
            ArtistPersons = await ToList(source.ArtistPersons(ct), ct),
            Lyrics = await ToList(source.Lyrics(ct), ct),
            Likes = await ToList(source.Likes(ct), ct),
            Playlists = await ToList(source.Playlists(ct), ct),
            PlaylistSongs = await ToList(source.PlaylistSongs(ct), ct),
            BlogPosts = await ToList(source.BlogPosts(ct), ct),
            Users = await ToList(source.Users(ct), ct),
            Roles = await ToList(source.Roles(ct), ct),
            UserRoles = await ToList(source.UserRoles(ct), ct),
            UserLogins = await ToList(source.UserLogins(ct), ct),
            UserClaims = await ToList(source.UserClaims(ct), ct),
            UserTokens = await ToList(source.UserTokens(ct), ct),
            DroppedTableCounts = await source.DroppedTableCountsAsync(ct),
        };
    }

    private static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> items, CancellationToken ct)
    {
        var list = new List<T>();
        await foreach (var item in items.WithCancellation(ct))
            list.Add(item);
        return list;
    }
}
