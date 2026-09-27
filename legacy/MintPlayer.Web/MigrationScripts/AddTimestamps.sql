ALTER TABLE [Subjects] ADD [DateDelete] datetime2 NULL;

GO

ALTER TABLE [Subjects] ADD [DateInsert] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [Subjects] ADD [DateUpdate] datetime2 NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200113130019_AddTimestamps', N'3.1.0');

GO

