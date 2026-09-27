SET NOCOUNT ON;
DECLARE @z uniqueidentifier = '00000000-0000-0000-0000-000000000000';
SELECT 'Subjects' t, SUM(CASE WHEN UserDeleteId = @z THEN 1 ELSE 0 END) zero_guid, SUM(CASE WHEN UserDeleteId IS NOT NULL AND UserDeleteId <> @z THEN 1 ELSE 0 END) real_user, SUM(CASE WHEN DateDelete IS NOT NULL THEN 1 ELSE 0 END) datedelete, SUM(CASE WHEN DateDelete = '0001-01-01' THEN 1 ELSE 0 END) datedelete_min, SUM(CASE WHEN DateInsert = '0001-01-01' THEN 1 ELSE 0 END) insert_min FROM mintplay.Subjects
UNION ALL SELECT 'MediumTypes', SUM(CASE WHEN UserDeleteId = @z THEN 1 ELSE 0 END), SUM(CASE WHEN UserDeleteId IS NOT NULL AND UserDeleteId <> @z THEN 1 ELSE 0 END), NULL, NULL, NULL FROM mintplay.MediumTypes
UNION ALL SELECT 'Tags', SUM(CASE WHEN UserDeleteId = @z THEN 1 ELSE 0 END), SUM(CASE WHEN UserDeleteId IS NOT NULL AND UserDeleteId <> @z THEN 1 ELSE 0 END), SUM(CASE WHEN DateDelete IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN DateDelete = '0001-01-01' THEN 1 ELSE 0 END), SUM(CASE WHEN DateInsert = '0001-01-01' THEN 1 ELSE 0 END) FROM mintplay.Tags
UNION ALL SELECT 'TagCategories', SUM(CASE WHEN UserDeleteId = @z THEN 1 ELSE 0 END), SUM(CASE WHEN UserDeleteId IS NOT NULL AND UserDeleteId <> @z THEN 1 ELSE 0 END), SUM(CASE WHEN DateDelete IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN DateDelete = '0001-01-01' THEN 1 ELSE 0 END), SUM(CASE WHEN DateInsert = '0001-01-01' THEN 1 ELSE 0 END) FROM mintplay.TagCategories
UNION ALL SELECT 'BlogPosts', SUM(CASE WHEN UserDeleteId = @z THEN 1 ELSE 0 END), SUM(CASE WHEN UserDeleteId IS NOT NULL AND UserDeleteId <> @z THEN 1 ELSE 0 END), SUM(CASE WHEN DateDelete IS NOT NULL THEN 1 ELSE 0 END), SUM(CASE WHEN DateDelete = '0001-01-01' THEN 1 ELSE 0 END), SUM(CASE WHEN DateInsert = '0001-01-01' THEN 1 ELSE 0 END) FROM mintplay.BlogPosts;
-- which rows count as deleted when zero-guid is treated as "not deleted"
SELECT SubjectType, SUM(CASE WHEN (UserDeleteId IS NOT NULL AND UserDeleteId <> @z) OR DateDelete IS NOT NULL THEN 1 ELSE 0 END) deleted_fixed FROM mintplay.Subjects GROUP BY SubjectType;
SELECT (SELECT COUNT(*) FROM mintplay.Tags WHERE (UserDeleteId IS NOT NULL AND UserDeleteId <> @z) OR DateDelete IS NOT NULL) tags_deleted_fixed,
       (SELECT COUNT(*) FROM mintplay.Tags WHERE ParentId IS NOT NULL) tags_with_parent_col,
       (SELECT COUNT(*) FROM mintplay.Tags WHERE ParentId IS NOT NULL AND ParentId <> 0) tags_parent_nonzero,
       (SELECT COUNT(*) FROM mintplay.Tags t WHERE t.ParentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mintplay.Tags p WHERE p.Id = t.ParentId)) tags_parent_missing,
       (SELECT COUNT(*) FROM mintplay.TagCategories WHERE (UserDeleteId IS NOT NULL AND UserDeleteId <> @z) OR DateDelete IS NOT NULL) cats_deleted_fixed,
       (SELECT COUNT(*) FROM mintplay.BlogPosts WHERE (UserDeleteId IS NOT NULL AND UserDeleteId <> @z) OR DateDelete IS NOT NULL) blog_deleted_fixed;
SELECT t.Id, t.CategoryId, CASE WHEN c.Id IS NULL THEN 'missing' ELSE 'ok' END cat FROM mintplay.Tags t LEFT JOIN mintplay.TagCategories c ON c.Id = t.CategoryId WHERE c.Id IS NULL;
-- lyrics-site links: type and subject kind
SELECT m.TypeId, s.SubjectType, LOWER(LEFT(SUBSTRING(m.Value, CHARINDEX('//', m.Value) + 2, 200), CHARINDEX('/', SUBSTRING(m.Value, CHARINDEX('//', m.Value) + 2, 200) + '/') - 1)) host, COUNT(*) n
FROM mintplay.Media m JOIN mintplay.Subjects s ON s.Id = m.SubjectId WHERE m.TypeId = 15 OR m.Value LIKE '%genius%' OR m.Value LIKE '%musixmatch%' OR m.Value LIKE '%songteksten%' OR m.Value LIKE '%lyrics%'
GROUP BY m.TypeId, s.SubjectType, LOWER(LEFT(SUBSTRING(m.Value, CHARINDEX('//', m.Value) + 2, 200), CHARINDEX('/', SUBSTRING(m.Value, CHARINDEX('//', m.Value) + 2, 200) + '/') - 1));
SELECT Value FROM mintplay.Media WHERE CHARINDEX('//', Value) = 0;
-- lyrics timeline vs line count
SELECT TOP 5 l.SongId, LEN(l.Timeline) - LEN(REPLACE(l.Timeline, ',', '')) + 1 timeline_entries, LEN(l.Text) - LEN(REPLACE(l.Text, CHAR(10), '')) + 1 text_lines FROM mintplay.Lyrics l WHERE l.Timeline NOT IN ('', '[]') AND l.Timeline IS NOT NULL;
