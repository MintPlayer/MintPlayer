BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'SubjectType');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Subjects] ALTER COLUMN [SubjectType] nvarchar(8) NOT NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Jobs]') AND [c].[name] = N'JobType');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Jobs] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Jobs] ALTER COLUMN [JobType] nvarchar(13) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218072728_UpgradeToNet10', N'10.0.0');

COMMIT;
GO

