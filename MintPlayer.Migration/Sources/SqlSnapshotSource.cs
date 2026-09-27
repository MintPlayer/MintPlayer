using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using MintPlayer.Migration.Model;

namespace MintPlayer.Migration.Sources;

/// <summary>
/// Reads a restored production backup (.bak) with plain SQL (Dapper + Microsoft.Data.SqlClient).
/// Column lists are explicit so a schema drift fails loudly instead of silently dropping data.
/// The schema name is configurable because production uses <c>mintplay</c>, not <c>dbo</c> (S2).
/// Read-only: only SELECTs are issued.
/// </summary>
public sealed partial class SqlSnapshotSource : ILegacySource
{
    private readonly string connectionString;
    private readonly string schema;

    public SqlSnapshotSource(string connectionString, string schema = "mintplay")
    {
        if (!IdentifierRegex().IsMatch(schema))
            throw new ArgumentException($"Invalid schema name '{schema}'.", nameof(schema));
        this.connectionString = connectionString;
        this.schema = schema;
    }

    public SourceCapabilities Capabilities => SourceCapabilities.All;

    public string Schema => schema;

    public IAsyncEnumerable<LegacySubject> Subjects(CancellationToken ct = default) => Query<LegacySubject>(
        "SELECT Id, SubjectType, UserDeleteId, DateInsert, DateUpdate, DateDelete, Name, YearStarted, YearQuit, " +
        "FirstName, LastName, Born, Died, Title, Released FROM {0}.Subjects ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyMedium> Media(CancellationToken ct = default) => Query<LegacyMedium>(
        "SELECT Id, TypeId, SubjectId, Value FROM {0}.Media ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyMediumType> MediumTypes(CancellationToken ct = default) => Query<LegacyMediumType>(
        "SELECT Id, Description, UserDeleteId, Visible FROM {0}.MediumTypes ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyTag> Tags(CancellationToken ct = default) => Query<LegacyTag>(
        "SELECT Id, Description, CategoryId, ParentId, UserDeleteId, DateInsert, DateUpdate, DateDelete FROM {0}.Tags ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyTagCategory> TagCategories(CancellationToken ct = default) => Query<LegacyTagCategory>(
        "SELECT Id, Color, Description, UserDeleteId, DateInsert, DateUpdate, DateDelete FROM {0}.TagCategories ORDER BY Id", ct);

    public IAsyncEnumerable<LegacySubjectTag> SubjectTags(CancellationToken ct = default) => Query<LegacySubjectTag>(
        "SELECT SubjectId, TagId FROM {0}.SubjectTag ORDER BY SubjectId, TagId", ct);

    public IAsyncEnumerable<LegacyArtistSong> ArtistSongs(CancellationToken ct = default) => Query<LegacyArtistSong>(
        "SELECT ArtistId, SongId, Credited FROM {0}.ArtistSong ORDER BY SongId, ArtistId", ct);

    public IAsyncEnumerable<LegacyArtistPerson> ArtistPersons(CancellationToken ct = default) => Query<LegacyArtistPerson>(
        "SELECT ArtistId, PersonId, Active FROM {0}.ArtistPerson ORDER BY ArtistId, PersonId", ct);

    public IAsyncEnumerable<LegacyLyrics> Lyrics(CancellationToken ct = default) => Query<LegacyLyrics>(
        "SELECT SongId, UserId, Text, UpdatedAt, Timeline FROM {0}.Lyrics ORDER BY SongId, UpdatedAt", ct);

    public IAsyncEnumerable<LegacyLike> Likes(CancellationToken ct = default) => Query<LegacyLike>(
        "SELECT SubjectId, UserId, DoesLike FROM {0}.Likes ORDER BY UserId, SubjectId", ct);

    public IAsyncEnumerable<LegacyPlaylist> Playlists(CancellationToken ct = default) => Query<LegacyPlaylist>(
        "SELECT Id, UserId, Description, IsDeleted, Accessibility FROM {0}.Playlists ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyPlaylistSong> PlaylistSongs(CancellationToken ct = default) => Query<LegacyPlaylistSong>(
        "SELECT PlaylistId, SongId, [Index] AS [Index] FROM {0}.PlaylistSong ORDER BY PlaylistId, [Index]", ct);

    public IAsyncEnumerable<LegacyBlogPost> BlogPosts(CancellationToken ct = default) => Query<LegacyBlogPost>(
        "SELECT Id, Title, Headline, Body, UserInsertId, UserDeleteId, DateInsert, DateUpdate, DateDelete FROM {0}.BlogPosts ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyUser> Users(CancellationToken ct = default) => Query<LegacyUser>(
        "SELECT Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, " +
        "PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, " +
        "PictureUrl, Bypass2faForExternalLogin FROM {0}.AspNetUsers ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyRole> Roles(CancellationToken ct = default) => Query<LegacyRole>(
        "SELECT Id, Name FROM {0}.AspNetRoles ORDER BY Name", ct);

    public IAsyncEnumerable<LegacyUserRole> UserRoles(CancellationToken ct = default) => Query<LegacyUserRole>(
        "SELECT UserId, RoleId FROM {0}.AspNetUserRoles ORDER BY UserId, RoleId", ct);

    public IAsyncEnumerable<LegacyUserLogin> UserLogins(CancellationToken ct = default) => Query<LegacyUserLogin>(
        "SELECT UserId, LoginProvider, ProviderKey, ProviderDisplayName FROM {0}.AspNetUserLogins ORDER BY UserId, LoginProvider", ct);

    public IAsyncEnumerable<LegacyUserClaim> UserClaims(CancellationToken ct = default) => Query<LegacyUserClaim>(
        "SELECT UserId, ClaimType, ClaimValue FROM {0}.AspNetUserClaims ORDER BY Id", ct);

    public IAsyncEnumerable<LegacyUserToken> UserTokens(CancellationToken ct = default) => Query<LegacyUserToken>(
        "SELECT UserId, LoginProvider, Name, Value FROM {0}.AspNetUserTokens ORDER BY UserId, LoginProvider, Name", ct);

    public async Task<Dictionary<string, int>> DroppedTableCountsAsync(CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        var result = new Dictionary<string, int>();
        foreach (var table in new[] { "Jobs", "LogEntries" })
        {
            result[table] = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition($"SELECT COUNT(*) FROM {schema}.{table}", cancellationToken: ct));
        }
        return result;
    }

    /// <summary>Scalar helper for the reconciler's independent source-side counts.</summary>
    public async Task<int> CountAsync(string sqlWithSchemaPlaceholder, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(connectionString);
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(string.Format(sqlWithSchemaPlaceholder, schema), cancellationToken: ct));
    }

    private async IAsyncEnumerable<T> Query<T>(string sql, [EnumeratorCancellation] CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await foreach (var row in connection.QueryUnbufferedAsync<T>(string.Format(sql, schema)).WithCancellation(ct))
            yield return row;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierRegex();
}
