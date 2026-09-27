using MintPlayer.Migration.Model;

namespace MintPlayer.Migration.Transform;

/// <summary>
/// §5.3 pre-flight: conditions that make the run fail before anything is written — unknown
/// discriminators, orphan foreign keys, tag cycles, duplicate normalised emails, lyrics on non-songs,
/// roles that are not a security.json group. Returns error strings (ids only).
/// </summary>
public static class Preflight
{
    private static readonly HashSet<string> KnownSubjectTypes = ["artist", "person", "song"];

    public static List<string> Check(LegacySnapshot s, IReadOnlySet<string> knownGroups)
    {
        var errors = new List<string>();
        var subjects = s.Subjects.ToDictionary(x => x.Id);
        var users = s.Users.Select(u => u.Id).ToHashSet();
        var tags = s.Tags.ToDictionary(t => t.Id);
        var categories = s.TagCategories.Select(c => c.Id).ToHashSet();
        var mediumTypes = s.MediumTypes.Select(m => m.Id).ToHashSet();
        var playlists = s.Playlists.Select(p => p.Id).ToHashSet();
        var roles = s.Roles.ToDictionary(r => r.Id);

        foreach (var x in s.Subjects.Where(x => !KnownSubjectTypes.Contains(x.SubjectType)))
            errors.Add($"Subjects/{x.Id}: unknown discriminator '{x.SubjectType}'");

        bool IsType(int id, string type) => subjects.TryGetValue(id, out var x) && x.SubjectType == type;

        foreach (var m in s.Media)
        {
            if (!mediumTypes.Contains(m.TypeId)) errors.Add($"Media/{m.Id}: TypeId {m.TypeId} does not exist");
            // an orphan medium (no subject) is reported + skipped by the transform, not fatal (§5.2)
        }
        foreach (var st in s.SubjectTags)
        {
            if (!subjects.ContainsKey(st.SubjectId)) errors.Add($"SubjectTag: subject {st.SubjectId} does not exist");
            if (!tags.ContainsKey(st.TagId)) errors.Add($"SubjectTag: tag {st.TagId} does not exist");
        }
        foreach (var a in s.ArtistSongs)
        {
            if (!IsType(a.ArtistId, "artist")) errors.Add($"ArtistSong: artist {a.ArtistId} missing or not an artist");
            if (!IsType(a.SongId, "song")) errors.Add($"ArtistSong: song {a.SongId} missing or not a song");
        }
        foreach (var a in s.ArtistPersons)
        {
            if (!IsType(a.ArtistId, "artist")) errors.Add($"ArtistPerson: artist {a.ArtistId} missing or not an artist");
            if (!IsType(a.PersonId, "person")) errors.Add($"ArtistPerson: person {a.PersonId} missing or not a person");
        }
        foreach (var t in s.Tags)
        {
            if (t.CategoryId != 0 && !categories.Contains(t.CategoryId)) errors.Add($"Tags/{t.Id}: CategoryId {t.CategoryId} does not exist");
            if (t.ParentId != 0 && !tags.ContainsKey(t.ParentId)) errors.Add($"Tags/{t.Id}: ParentId {t.ParentId} does not exist");
        }
        foreach (var t in s.Tags)
        {
            var seen = new HashSet<int> { t.Id };
            for (var p = t.ParentId; p != 0 && tags.TryGetValue(p, out var parent); p = parent.ParentId)
            {
                if (!seen.Add(p)) { errors.Add($"Tags/{t.Id}: parent cycle"); break; }
            }
        }
        foreach (var l in s.Lyrics)
        {
            if (!IsType(l.SongId, "song")) errors.Add($"Lyrics: song {l.SongId} missing or not a song");
            try { LyricsTransform.ParseTimeline(l.Timeline); }
            catch (Exception) { errors.Add($"Lyrics: song {l.SongId} has an unparseable timeline"); }
        }
        foreach (var l in s.Likes)
        {
            if (!subjects.ContainsKey(l.SubjectId)) errors.Add($"Likes: subject {l.SubjectId} does not exist");
            if (!users.Contains(l.UserId)) errors.Add($"Likes: user {l.UserId} does not exist");
        }
        foreach (var p in s.Playlists.Where(p => !users.Contains(p.UserId)))
            errors.Add($"Playlists/{p.Id}: owner {p.UserId} does not exist");
        foreach (var ps in s.PlaylistSongs)
        {
            if (!playlists.Contains(ps.PlaylistId)) errors.Add($"PlaylistSong: playlist {ps.PlaylistId} does not exist");
            if (!IsType(ps.SongId, "song")) errors.Add($"PlaylistSong: song {ps.SongId} missing or not a song");
        }
        foreach (var b in s.BlogPosts.Where(b => !users.Contains(b.UserInsertId)))
            errors.Add($"BlogPosts/{b.Id}: author {b.UserInsertId} does not exist");

        foreach (var dup in s.Users
                     .Where(u => u.Email != null)
                     .GroupBy(u => Ids.EmailReservation(u.NormalizedEmail ?? u.Email!.ToUpperInvariant()))
                     .Where(g => g.Count() > 1))
            errors.Add($"AspNetUsers: {dup.Count()} users share one normalised email (ids {string.Join(", ", dup.Select(u => u.Id))})");
        foreach (var dup in s.Users.Where(u => u.NormalizedUserName != null).GroupBy(u => u.NormalizedUserName).Where(g => g.Count() > 1))
            errors.Add($"AspNetUsers: {dup.Count()} users share one normalised user name (ids {string.Join(", ", dup.Select(u => u.Id))})");

        foreach (var ur in s.UserRoles)
        {
            if (!users.Contains(ur.UserId)) errors.Add($"AspNetUserRoles: user {ur.UserId} does not exist");
            if (!roles.TryGetValue(ur.RoleId, out var role)) errors.Add($"AspNetUserRoles: role {ur.RoleId} does not exist");
            else if (role.Name is null || !knownGroups.Contains(role.Name)) errors.Add($"AspNetRoles/{ur.RoleId}: role is not a security.json group");
        }
        foreach (var x in s.UserLogins.Where(x => !users.Contains(x.UserId))) errors.Add($"AspNetUserLogins: user {x.UserId} does not exist");
        foreach (var x in s.UserClaims.Where(x => !users.Contains(x.UserId))) errors.Add($"AspNetUserClaims: user {x.UserId} does not exist");
        foreach (var x in s.UserTokens.Where(x => !users.Contains(x.UserId))) errors.Add($"AspNetUserTokens: user {x.UserId} does not exist");

        return errors;
    }
}
