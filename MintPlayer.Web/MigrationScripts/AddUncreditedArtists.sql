ALTER TABLE [ArtistSong] ADD [Credited] bit NOT NULL DEFAULT CAST(1 AS bit);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210225175302_AddUncreditedArtists', N'3.1.3');

GO

