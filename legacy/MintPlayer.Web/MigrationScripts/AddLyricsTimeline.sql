ALTER TABLE [Lyrics] ADD [Timeline] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200331091735_AddLyricsTimeline', N'3.1.3');

GO
