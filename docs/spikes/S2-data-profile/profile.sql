SET NOCOUNT ON;
PRINT '== Subjects by type / deleted';
SELECT SubjectType, COUNT(*) n,
  SUM(CASE WHEN UserDeleteId IS NOT NULL OR DateDelete IS NOT NULL THEN 1 ELSE 0 END) deleted,
  SUM(CASE WHEN DateDelete IS NOT NULL AND UserDeleteId IS NULL THEN 1 ELSE 0 END) deleted_no_user,
  SUM(CASE WHEN Released = '0001-01-01' THEN 1 ELSE 0 END) released_min,
  SUM(CASE WHEN DateInsert IS NULL THEN 1 ELSE 0 END) no_dateinsert,
  MIN(DateInsert) first_insert, MAX(DateUpdate) last_update
FROM mintplay.Subjects GROUP BY SubjectType;
GO
PRINT '== Media by type';
SELECT mt.Id, mt.Description, mt.Visible, CASE WHEN mt.UserDeleteId IS NULL THEN 0 ELSE 1 END type_deleted, COUNT(m.Id) media
FROM mintplay.MediumTypes mt LEFT JOIN mintplay.Media m ON m.TypeId = mt.Id
GROUP BY mt.Id, mt.Description, mt.Visible, mt.UserDeleteId ORDER BY mt.Id;
SELECT SUM(CASE WHEN m.SubjectId IS NULL THEN 1 ELSE 0 END) orphan_null_subject,
       SUM(CASE WHEN m.SubjectId IS NOT NULL AND s.Id IS NULL THEN 1 ELSE 0 END) orphan_missing_subject,
       SUM(CASE WHEN m.TypeId IS NULL THEN 1 ELSE 0 END) no_type,
       SUM(CASE WHEN s.UserDeleteId IS NOT NULL OR s.DateDelete IS NOT NULL THEN 1 ELSE 0 END) on_deleted_subject
FROM mintplay.Media m LEFT JOIN mintplay.Subjects s ON s.Id = m.SubjectId;
GO
PRINT '== Media hosts';
SELECT TOP 25 host, COUNT(*) n FROM (
  SELECT LOWER(SUBSTRING(Value, CHARINDEX('//', Value) + 2, CHARINDEX('/', Value + '/', CHARINDEX('//', Value) + 2) - CHARINDEX('//', Value) - 2)) host
  FROM mintplay.Media WHERE CHARINDEX('//', Value) > 0) x GROUP BY host ORDER BY n DESC;
SELECT COUNT(*) media_without_scheme FROM mintplay.Media WHERE CHARINDEX('//', Value) = 0;
GO
PRINT '== Lyrics';
SELECT COUNT(*) rows_, COUNT(DISTINCT l.SongId) songs,
  SUM(CASE WHEN l.Timeline IS NOT NULL AND l.Timeline NOT IN ('', '[]') THEN 1 ELSE 0 END) with_timeline,
  SUM(CASE WHEN s.Id IS NULL THEN 1 ELSE 0 END) not_on_song,
  SUM(CASE WHEN l.Text IS NULL OR l.Text = '' THEN 1 ELSE 0 END) empty_text,
  SUM(CASE WHEN l.Text LIKE '%' + CHAR(13) + '%' THEN 1 ELSE 0 END) crlf_text
FROM mintplay.Lyrics l LEFT JOIN mintplay.Subjects s ON s.Id = l.SongId AND s.SubjectType = 'song';
SELECT versions, COUNT(*) songs FROM (SELECT SongId, COUNT(*) versions FROM mintplay.Lyrics GROUP BY SongId) x GROUP BY versions;
SELECT TOP 3 LEFT(Timeline, 60) timeline_sample FROM mintplay.Lyrics WHERE Timeline IS NOT NULL AND Timeline NOT IN ('', '[]');
GO
PRINT '== Tags / categories';
SELECT COUNT(*) cats, SUM(CASE WHEN (CAST(Color AS bigint) & 0xFF000000) <> 0xFF000000 THEN 1 ELSE 0 END) alpha_not_255,
  SUM(CASE WHEN UserDeleteId IS NOT NULL OR DateDelete IS NOT NULL THEN 1 ELSE 0 END) deleted FROM mintplay.TagCategories;
SELECT COUNT(*) tags, SUM(CASE WHEN t.ParentId IS NOT NULL THEN 1 ELSE 0 END) with_parent,
  SUM(CASE WHEN t.UserDeleteId IS NOT NULL OR t.DateDelete IS NOT NULL THEN 1 ELSE 0 END) deleted,
  SUM(CASE WHEN c.Id IS NULL THEN 1 ELSE 0 END) bad_category
FROM mintplay.Tags t LEFT JOIN mintplay.TagCategories c ON c.Id = t.CategoryId;
WITH r AS (SELECT Id, ParentId, 0 d FROM mintplay.Tags UNION ALL SELECT r.Id, t.ParentId, r.d + 1 FROM r JOIN mintplay.Tags t ON t.Id = r.ParentId WHERE r.d < 60 AND r.ParentId IS NOT NULL)
SELECT MAX(d) max_depth, SUM(CASE WHEN d >= 60 THEN 1 ELSE 0 END) cycle_suspects FROM r OPTION (MAXRECURSION 100);
SELECT COUNT(*) subjecttag_orphans FROM mintplay.SubjectTag st LEFT JOIN mintplay.Subjects s ON s.Id = st.SubjectId LEFT JOIN mintplay.Tags t ON t.Id = st.TagId WHERE s.Id IS NULL OR t.Id IS NULL;
GO
PRINT '== Playlists';
SELECT p.Accessibility, p.IsDeleted, COUNT(*) n, SUM(CASE WHEN p.UserId IS NULL THEN 1 ELSE 0 END) no_owner,
  SUM(CASE WHEN p.UserId IS NOT NULL AND u.Id IS NULL THEN 1 ELSE 0 END) owner_missing
FROM mintplay.Playlists p LEFT JOIN mintplay.AspNetUsers u ON u.Id = p.UserId GROUP BY p.Accessibility, p.IsDeleted;
SELECT COUNT(*) dup_track_pairs FROM (SELECT PlaylistId, SongId FROM mintplay.PlaylistSong GROUP BY PlaylistId, SongId HAVING COUNT(*) > 1) x;
SELECT COUNT(*) track_orphans FROM mintplay.PlaylistSong ps LEFT JOIN mintplay.Subjects s ON s.Id = ps.SongId AND s.SubjectType = 'song' WHERE s.Id IS NULL;
GO
PRINT '== Likes / join orphans';
SELECT COUNT(DISTINCT l.UserId) users, SUM(CASE WHEN l.DoesLike = 1 THEN 1 ELSE 0 END) likes, SUM(CASE WHEN l.DoesLike = 0 THEN 1 ELSE 0 END) dislikes,
  SUM(CASE WHEN u.Id IS NULL THEN 1 ELSE 0 END) user_missing, SUM(CASE WHEN s.Id IS NULL THEN 1 ELSE 0 END) subject_missing
FROM mintplay.Likes l LEFT JOIN mintplay.AspNetUsers u ON u.Id = l.UserId LEFT JOIN mintplay.Subjects s ON s.Id = l.SubjectId;
SELECT COUNT(*) artistsong_orphans FROM mintplay.ArtistSong a LEFT JOIN mintplay.Subjects x ON x.Id = a.ArtistId AND x.SubjectType = 'artist' LEFT JOIN mintplay.Subjects y ON y.Id = a.SongId AND y.SubjectType = 'song' WHERE x.Id IS NULL OR y.Id IS NULL;
SELECT COUNT(*) uncredited FROM mintplay.ArtistSong WHERE Credited = 0;
SELECT COUNT(*) artistperson_orphans FROM mintplay.ArtistPerson a LEFT JOIN mintplay.Subjects x ON x.Id = a.ArtistId AND x.SubjectType = 'artist' LEFT JOIN mintplay.Subjects y ON y.Id = a.PersonId AND y.SubjectType = 'person' WHERE x.Id IS NULL OR y.Id IS NULL;
GO
PRINT '== Blog';
SELECT COUNT(*) n, SUM(CASE WHEN UserDeleteId IS NOT NULL OR DateDelete IS NOT NULL THEN 1 ELSE 0 END) deleted,
  SUM(CASE WHEN UserInsertId IS NULL THEN 1 ELSE 0 END) no_author, MIN(DateInsert) first_, MAX(DateInsert) last_ FROM mintplay.BlogPosts;
GO
PRINT '== Users (aggregates only)';
SELECT COUNT(*) users, SUM(CASE WHEN PasswordHash IS NULL THEN 1 ELSE 0 END) no_password,
  SUM(CASE WHEN EmailConfirmed = 1 THEN 1 ELSE 0 END) email_confirmed, SUM(CASE WHEN TwoFactorEnabled = 1 THEN 1 ELSE 0 END) twofa,
  SUM(CASE WHEN Bypass2faForExternalLogin = 1 THEN 1 ELSE 0 END) bypass2fa, SUM(CASE WHEN LockoutEnd > SYSDATETIMEOFFSET() THEN 1 ELSE 0 END) locked_now,
  SUM(CASE WHEN NormalizedEmail IS NULL THEN 1 ELSE 0 END) no_email, SUM(CASE WHEN PictureUrl IS NOT NULL THEN 1 ELSE 0 END) with_picture,
  SUM(CASE WHEN LEFT(PasswordHash, 4) = 'AQAA' THEN 1 ELSE 0 END) hash_v3
FROM mintplay.AspNetUsers;
SELECT COUNT(*) dup_normalized_emails FROM (SELECT NormalizedEmail FROM mintplay.AspNetUsers WHERE NormalizedEmail IS NOT NULL GROUP BY NormalizedEmail HAVING COUNT(*) > 1) x;
SELECT COUNT(*) dup_normalized_usernames FROM (SELECT NormalizedUserName FROM mintplay.AspNetUsers WHERE NormalizedUserName IS NOT NULL GROUP BY NormalizedUserName HAVING COUNT(*) > 1) x;
SELECT r.Name role, COUNT(ur.UserId) members FROM mintplay.AspNetRoles r LEFT JOIN mintplay.AspNetUserRoles ur ON ur.RoleId = r.Id GROUP BY r.Name;
SELECT LoginProvider, COUNT(*) n FROM mintplay.AspNetUserLogins GROUP BY LoginProvider;
SELECT LoginProvider, Name, COUNT(*) n FROM mintplay.AspNetUserTokens GROUP BY LoginProvider, Name;
SELECT COUNT(*) users_with_activity FROM mintplay.AspNetUsers u
WHERE u.EmailConfirmed = 1 OR u.Id IN (SELECT UserId FROM mintplay.Likes) OR u.Id IN (SELECT UserId FROM mintplay.Playlists WHERE UserId IS NOT NULL) OR u.Id IN (SELECT UserId FROM mintplay.AspNetUserLogins);
GO
PRINT '== Jobs';
SELECT JobType, Status, COUNT(*) n FROM mintplay.Jobs GROUP BY JobType, Status;
