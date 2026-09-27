using MintPlayer.Domain.Entities;
using MintPlayer.Migration.Model;
using MintPlayer.Migration.Target;
using MintPlayer.Spark.Authorization.Identity;
using MintPlayer.Web;

namespace MintPlayer.Migration.Transform;

/// <summary>
/// The whole legacy → RavenDB mapping of §5.2 as one pure function: snapshot in, <see cref="MigrationPlan"/>
/// out. No I/O, no clock (the migration timestamp comes in via <see cref="TransformOptions"/>).
/// </summary>
public static class LegacyTransformer
{
    public const string AuthenticatorKeyTokenName = "AuthenticatorKey";
    public const string RecoveryCodesTokenName = "RecoveryCodes";
    public const string IdentityTokenProvider = "[AspNetUserStore]";
    /// <summary>Group membership claim read by Spark's <c>ClaimsGroupMembershipProvider</c> and by the
    /// app's own <c>User.HasClaim("group", …)</c> checks (see <c>DevDataSeeder</c>).</summary>
    public const string GroupClaimType = "group";

    public static MigrationPlan Transform(LegacySnapshot s, TransformOptions o)
    {
        var plan = new MigrationPlan();
        var userIds = s.Users.Select(u => u.Id).ToHashSet();
        var subjectTypes = s.Subjects.ToDictionary(x => x.Id, x => x.SubjectType);

        MapMediumTypes(s, o, plan, userIds);
        MapTagCategories(s, o, plan, userIds);
        MapTags(s, o, plan, userIds);
        MapSubjects(s, o, plan, userIds, subjectTypes);
        MapPlaylists(s, o, plan);
        MapBlogPosts(s, o, plan, userIds);
        MapUsers(s, o, plan);
        MapLikes(s, plan, subjectTypes);

        foreach (var (table, count) in s.DroppedTableCounts)
            plan.Issue(IssueSeverity.Info, "table-dropped", table, $"{count} rows not migrated by design");
        return plan;
    }

    private static (DateTimeOffset CreatedAt, DateTimeOffset? ModifiedAt) Audit(
        string id, DateTime dateInsert, DateTime? dateUpdate, TransformOptions o, MigrationPlan plan)
    {
        var modified = LegacyTime.ToUtcOrNull(dateUpdate, o.TimeZone);
        if (!LegacyTime.IsMin(dateInsert))
            return (LegacyTime.ToUtc(dateInsert, o.TimeZone), modified);

        plan.Issue(IssueSeverity.Info, "created-fallback", id,
            modified is null ? "DateInsert 0001-01-01, no DateUpdate → migration time" : "DateInsert 0001-01-01 → DateUpdate");
        return (modified ?? o.MigratedAt, modified);
    }

    private static (bool IsDeleted, DateTimeOffset? DeletedAt) Deletion(
        string id, Guid? userDeleteId, DateTime? dateDelete, IReadOnlySet<Guid> userIds, TransformOptions o, MigrationPlan plan)
    {
        var deleted = SoftDelete.IsDeleted(userDeleteId, userIds);
        if (deleted)
        {
            var at = LegacyTime.ToUtcOrNull(dateDelete, o.TimeZone);
            if (at is null)
                plan.Issue(IssueSeverity.Info, "deleted-without-date", id, "deleted by a user but no DateDelete → DeletedAt null");
            return (true, at);
        }
        if (dateDelete is not null)
            plan.Issue(IssueSeverity.Warning, "bare-datedelete-live", id, "DateDelete set without a deleting user → live (legacy semantics), DeletedAt null");
        return (false, null);
    }

    private static void MapMediumTypes(LegacySnapshot s, TransformOptions o, MigrationPlan plan, IReadOnlySet<Guid> userIds)
    {
        foreach (var mt in s.MediumTypes)
        {
            var id = Ids.MediumType(mt.Id);
            var (deleted, _) = Deletion(id, mt.UserDeleteId, null, userIds, o, plan);
            plan.Add(id, "MediumTypes", new MediumType
            {
                Id = id,
                Name = mt.Description,
                Description = null,
                Visible = mt.Visible,
                CreatedAt = o.MigratedAt, // MediumTypes has no audit dates in production
                IsDeleted = deleted,
                OldId = mt.Id,
            });
            if (!mt.Visible)
                plan.Issue(IssueSeverity.Info, "mediumtype-hidden", id, "Visible = false (D15)");
        }
    }

    private static void MapTagCategories(LegacySnapshot s, TransformOptions o, MigrationPlan plan, IReadOnlySet<Guid> userIds)
    {
        foreach (var c in s.TagCategories)
        {
            var id = Ids.TagCategory(c.Id);
            var (created, modified) = Audit(id, c.DateInsert, c.DateUpdate, o, plan);
            var (deleted, deletedAt) = Deletion(id, c.UserDeleteId, c.DateDelete, userIds, o, plan);
            var color = Colors.FromLegacy(c.Color, out var hadAlpha);
            if (hadAlpha)
                plan.Issue(IssueSeverity.Info, "color-alpha-dropped", id, $"alpha {(byte)(c.Color >> 24)} → 255 (lost by design)");
            plan.Add(id, "TagCategories", new TagCategory
            {
                Id = id,
                Description = c.Description,
                Color = color,
                CreatedAt = created,
                ModifiedAt = modified,
                IsDeleted = deleted,
                DeletedAt = deletedAt,
                OldId = c.Id,
            });
        }
    }

    private static void MapTags(LegacySnapshot s, TransformOptions o, MigrationPlan plan, IReadOnlySet<Guid> userIds)
    {
        foreach (var t in s.Tags)
        {
            var id = Ids.Tag(t.Id);
            var (created, modified) = Audit(id, t.DateInsert, t.DateUpdate, o, plan);
            var (deleted, deletedAt) = Deletion(id, t.UserDeleteId, t.DateDelete, userIds, o, plan);
            if (t.CategoryId == 0)
                plan.Issue(IssueSeverity.Info, "tag-no-category", id, "CategoryId 0 → null");
            plan.Add(id, "Tags", new Tag
            {
                Id = id,
                Description = t.Description,
                CategoryId = t.CategoryId == 0 ? null : Ids.TagCategory(t.CategoryId),
                ParentId = t.ParentId == 0 ? null : Ids.Tag(t.ParentId),
                CreatedAt = created,
                ModifiedAt = modified,
                IsDeleted = deleted,
                DeletedAt = deletedAt,
                OldId = t.Id,
            });
        }
    }

    private static void MapSubjects(LegacySnapshot s, TransformOptions o, MigrationPlan plan, IReadOnlySet<Guid> userIds, IReadOnlyDictionary<int, string> subjectTypes)
    {
        var media = s.Media.GroupBy(m => m.SubjectId).ToDictionary(g => g.Key, g => g.OrderBy(m => m.Id).ToList());
        var tagsBySubject = s.SubjectTags.GroupBy(x => x.SubjectId).ToDictionary(g => g.Key, g => g.Select(x => x.TagId).Distinct().Order().ToList());
        var songArtists = s.ArtistSongs.GroupBy(x => x.SongId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.ArtistId).ToList());
        var members = s.ArtistPersons.GroupBy(x => x.ArtistId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.PersonId).ToList());
        var lyrics = s.Lyrics.GroupBy(x => x.SongId).ToDictionary(g => g.Key, g => (IReadOnlyList<LegacyLyrics>)g.ToList());

        foreach (var orphan in s.Media.Where(m => !subjectTypes.ContainsKey(m.SubjectId)))
            plan.Issue(IssueSeverity.Warning, "medium-orphan", $"Media/{orphan.Id}", $"subject {orphan.SubjectId} missing → skipped");
        foreach (var dup in s.SubjectTags.GroupBy(x => (x.SubjectId, x.TagId)).Where(g => g.Count() > 1))
            plan.Issue(IssueSeverity.Info, "subjecttag-duplicate", Ids.Subject(subjectTypes[dup.Key.SubjectId], dup.Key.SubjectId), $"tag {dup.Key.TagId} deduplicated");

        foreach (var x in s.Subjects)
        {
            var id = Ids.Subject(x.SubjectType, x.Id);
            var (created, modified) = Audit(id, x.DateInsert, x.DateUpdate, o, plan);
            var (deleted, deletedAt) = Deletion(id, x.UserDeleteId, x.DateDelete, userIds, o, plan);

            var subjectMedia = new List<Medium>();
            foreach (var m in media.GetValueOrDefault(x.Id) ?? [])
            {
                if (string.IsNullOrWhiteSpace(m.Value))
                {
                    plan.Issue(IssueSeverity.Warning, "medium-empty", $"Media/{m.Id}", $"empty Value on {id} → skipped");
                    continue;
                }
                subjectMedia.Add(new Medium { Value = m.Value, TypeId = Ids.MediumType(m.TypeId) });
            }
            var tagIds = (tagsBySubject.GetValueOrDefault(x.Id) ?? []).Select(Ids.Tag).ToList();

            Subject subject;
            switch (x.SubjectType)
            {
                case "artist":
                    subject = new Artist
                    {
                        Name = x.Name ?? string.Empty,
                        YearStarted = x.YearStarted,
                        YearQuit = x.YearQuit,
                        Members = (members.GetValueOrDefault(x.Id) ?? [])
                            .Select(m => new ArtistMember { PersonId = Ids.Person(m.PersonId), Active = m.Active }).ToList(),
                    };
                    break;
                case "person":
                    var born = LegacyTime.ToDate(x.Born, o.TimeZone, out var bornShifted);
                    var died = LegacyTime.ToDate(x.Died, o.TimeZone, out var diedShifted);
                    if (bornShifted) plan.Issue(IssueSeverity.Info, "date-had-time", id, "Born had a time part → read as UTC instant, local date taken");
                    if (diedShifted) plan.Issue(IssueSeverity.Info, "date-had-time", id, "Died had a time part → read as UTC instant, local date taken");
                    subject = new Person
                    {
                        FirstName = x.FirstName ?? string.Empty,
                        LastName = x.LastName ?? string.Empty,
                        Born = born,
                        Died = died,
                    };
                    break;
                case "song":
                    var released = LegacyTime.ToDate(x.Released, o.TimeZone, out var relShifted);
                    if (x.Released is { } r && LegacyTime.IsMin(r))
                        plan.Issue(IssueSeverity.Info, "released-min", id, "Released 0001-01-01 → null");
                    if (relShifted) plan.Issue(IssueSeverity.Info, "date-had-time", id, "Released had a time part → read as UTC instant, local date taken");
                    var song = new Song
                    {
                        Title = x.Title ?? string.Empty,
                        Released = released,
                        Artists = (songArtists.GetValueOrDefault(x.Id) ?? [])
                            .Select(a => new SongArtist { ArtistId = Ids.Artist(a.ArtistId), Credited = a.Credited }).ToList(),
                    };
                    ApplyLyrics(song, id, lyrics.GetValueOrDefault(x.Id), subjectMedia, plan);
                    subject = song;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown subject discriminator '{x.SubjectType}' on subject {x.Id}.");
            }

            subject.Id = id;
            subject.Media = subjectMedia;
            subject.TagIds = tagIds;
            subject.CreatedAt = created;
            subject.ModifiedAt = modified;
            subject.IsDeleted = deleted;
            subject.DeletedAt = deletedAt;
            subject.OldId = x.Id;
            plan.Add(id, CollectionOf(x.SubjectType), subject);
        }
    }

    private static void ApplyLyrics(Song song, string id, IReadOnlyList<LegacyLyrics>? rows, List<Medium> media, MigrationPlan plan)
    {
        if (rows is null || rows.Count == 0)
            return;
        var result = LyricsTransform.Collapse(rows);
        if (rows.Count > 1)
            plan.Issue(result.TextsDiffered ? IssueSeverity.Warning : IssueSeverity.Info, "lyrics-versions", id,
                $"{rows.Count} legacy versions collapsed to one{(result.TextsDiffered ? " — TEXTS DIFFER, latest non-empty kept" : " (identical text)")}");
        if (result.Text is null)
        {
            plan.Issue(IssueSeverity.Info, "lyrics-empty", id, result.LegacyEntries > 0 ? $"empty text; {result.LegacyEntries}-entry timeline dropped" : "empty text → Lyrics null");
            return;
        }
        song.Lyrics = result.Text;
        if (result.StartTimes is null)
            return;

        var url = Playability.FirstPlayable(media.Select(m => m.Value));
        if (url is null)
        {
            plan.Issue(IssueSeverity.Warning, "timeline-no-playable", id, $"{result.LegacyEntries}-entry timeline has no playable medium to key on → dropped");
            return;
        }
        song.LyricsTimings = [new LyricsTiming { MediumUrl = url, StartTimes = [.. result.StartTimes] }];
        plan.Issue(IssueSeverity.Info, "timeline-migrated", id,
            $"{result.LegacyEntries} legacy entries → {result.SyncedLines} synced of {result.StartTimes.Count} lines" +
            (result.DroppedEntries > 0 ? $", {result.DroppedEntries} surplus entries dropped" : ""));
    }

    private static string CollectionOf(string subjectType) => subjectType switch
    {
        "artist" => "Artists",
        "person" => "People",
        "song" => "Songs",
        _ => throw new InvalidOperationException(subjectType),
    };

    private static void MapPlaylists(LegacySnapshot s, TransformOptions o, MigrationPlan plan)
    {
        var tracks = s.PlaylistSongs.GroupBy(x => x.PlaylistId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Index).ThenBy(x => x.SongId).ToList());
        foreach (var p in s.Playlists)
        {
            var id = Ids.Playlist(p.Id);
            var list = tracks.GetValueOrDefault(p.Id) ?? [];
            var duplicates = list.Count - list.Select(x => x.SongId).Distinct().Count();
            if (duplicates > 0)
                plan.Issue(IssueSeverity.Info, "playlist-duplicate-track", id, $"{duplicates} duplicate track(s) kept");
            plan.Add(id, "Playlists", new Playlist
            {
                Id = id,
                Name = p.Description,
                Description = null,
                IsPublic = p.Accessibility == 1,
                OwnerId = Ids.User(p.UserId),
                Tracks = list.Select(x => new PlaylistTrack { SongId = Ids.Song(x.SongId) }).ToList(),
                CreatedAt = o.MigratedAt, // Playlists has no audit dates in production
                IsDeleted = p.IsDeleted,
                OldId = p.Id,
            });
        }
    }

    private static void MapBlogPosts(LegacySnapshot s, TransformOptions o, MigrationPlan plan, IReadOnlySet<Guid> userIds)
    {
        foreach (var b in s.BlogPosts)
        {
            var id = Ids.BlogPost(b.Id);
            var (created, modified) = Audit(id, b.DateInsert, b.DateUpdate, o, plan);
            var (deleted, deletedAt) = Deletion(id, b.UserDeleteId, b.DateDelete, userIds, o, plan);
            plan.Add(id, "BlogPosts", new BlogPost
            {
                Id = id,
                Title = b.Title,
                Headline = b.Headline,
                Body = b.Body,
                AuthorId = Ids.User(b.UserInsertId),
                Published = created,
                CreatedAt = created,
                ModifiedAt = modified,
                IsDeleted = deleted,
                DeletedAt = deletedAt,
                OldId = b.Id,
            });
        }
        if (s.BlogPosts.Count > 0)
            plan.Issue(IssueSeverity.Warning, "blogpost-stopgap-type", "BlogPosts",
                $"{s.BlogPosts.Count} posts written with the migration's stopgap BlogPost type — replace with the F1 domain entity");
    }

    private static void MapUsers(LegacySnapshot s, TransformOptions o, MigrationPlan plan)
    {
        var roles = s.Roles.ToDictionary(r => r.Id, r => r.Name);
        var userRoles = s.UserRoles.ToLookup(x => x.UserId);
        var logins = s.UserLogins.ToLookup(x => x.UserId);
        var claims = s.UserClaims.ToLookup(x => x.UserId);
        var tokens = s.UserTokens.ToLookup(x => x.UserId);

        foreach (var u in s.Users)
        {
            var id = Ids.User(u.Id);
            var user = new MintPlayerUser
            {
                Id = id,
                UserName = u.UserName,
                NormalizedUserName = u.NormalizedUserName,
                Email = u.Email,
                NormalizedEmail = u.NormalizedEmail ?? u.Email?.ToUpperInvariant(),
                EmailConfirmed = u.EmailConfirmed,
                PasswordHash = u.PasswordHash,       // verbatim (D8): Identity v3 hashes validate unchanged
                SecurityStamp = u.SecurityStamp,     // verbatim (D8)
                PhoneNumber = u.PhoneNumber,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled,
                LockoutEnd = u.LockoutEnd,
                LockoutEnabled = u.LockoutEnabled,
                AccessFailedCount = u.AccessFailedCount,
                PictureUrl = string.IsNullOrEmpty(u.PictureUrl) ? null : u.PictureUrl,
                Bypass2faForExternalLogin = u.Bypass2faForExternalLogin,
            };

            user.Logins = logins[u.Id]
                .Select(l => new SparkUserLogin { LoginProvider = l.LoginProvider, ProviderKey = l.ProviderKey, ProviderDisplayName = l.ProviderDisplayName })
                .ToList();
            user.Claims = claims[u.Id]
                .Where(c => c.ClaimType != null)
                .Select(c => new SparkUserClaim { ClaimType = c.ClaimType!, ClaimValue = c.ClaimValue ?? string.Empty })
                .ToList();
            foreach (var roleName in userRoles[u.Id].Select(r => roles.GetValueOrDefault(r.RoleId)).OfType<string>().Order(StringComparer.Ordinal))
            {
                if (!o.KnownGroups.Contains(roleName))
                    throw new InvalidOperationException($"Role on {id} is not a security.json group (pre-flight should have caught this).");
                user.Claims.Add(new SparkUserClaim { ClaimType = GroupClaimType, ClaimValue = roleName });
                plan.Issue(IssueSeverity.Info, "role-to-group-claim", id, $"role {roleName} → group claim");
            }

            foreach (var t in tokens[u.Id])
            {
                if (t.LoginProvider == IdentityTokenProvider && t.Name == AuthenticatorKeyTokenName)
                {
                    user.AuthenticatorKey = t.Value; // verbatim: existing authenticator apps keep working
                }
                else if (t.LoginProvider == IdentityTokenProvider && t.Name == RecoveryCodesTokenName)
                {
                    var codes = RecoveryCodes.Split(t.Value);
                    user.TwoFactorRecoveryCodes = o.HashRecoveryCodes ? codes.Select(RecoveryCodes.Hash).ToList() : codes.ToList();
                }
                else
                {
                    user.Tokens.Add(new SparkUserToken { LoginProvider = t.LoginProvider, Name = t.Name, Value = t.Value });
                }
            }
            if (user.AuthenticatorKey != null && !user.TwoFactorEnabled)
                plan.Issue(IssueSeverity.Info, "authenticator-unfinished", id, "authenticator key without 2FA enabled (enrolment never finished) — kept");

            plan.Add(id, "MintPlayerUsers", user);
            if (user.NormalizedEmail != null)
                plan.CompareExchange[Ids.EmailReservation(user.NormalizedEmail)] = id;
            else
                plan.Issue(IssueSeverity.Warning, "user-no-email", id, "no email → no emails/ reservation");
        }
    }

    private static void MapLikes(LegacySnapshot s, MigrationPlan plan, IReadOnlyDictionary<int, string> subjectTypes)
    {
        foreach (var g in s.Likes.GroupBy(l => l.UserId).OrderBy(g => g.Key))
        {
            var userId = Ids.User(g.Key);
            var id = Ids.UserLikes(userId);
            string SubjectId(LegacyLike l) => Ids.Subject(subjectTypes[l.SubjectId], l.SubjectId);
            plan.Add(id, "UserLikes", new UserLike
            {
                Id = id,
                UserId = userId,
                Likes = g.Where(l => l.DoesLike).OrderBy(l => l.SubjectId).Select(SubjectId).ToList(),
                Dislikes = g.Where(l => !l.DoesLike).OrderBy(l => l.SubjectId).Select(SubjectId).ToList(),
            });
        }
    }
}
