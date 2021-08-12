DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MediumTypes]') AND [c].[name] = N'PlayerType');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [MediumTypes] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [MediumTypes] DROP COLUMN [PlayerType];

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210812100810_RemovedPlayerType', N'3.1.17');

GO

