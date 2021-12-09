ALTER TABLE [Playlists] ADD [Accessibility] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210111223453_AddPlaylistAccessibility', N'3.1.3');

GO

