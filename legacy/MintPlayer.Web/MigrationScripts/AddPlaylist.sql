CREATE TABLE [Playlists] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NULL,
    [Description] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Playlists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Playlists_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [PlaylistSong] (
    [PlaylistId] int NOT NULL,
    [SongId] int NOT NULL,
    [Index] int NOT NULL,
    CONSTRAINT [PK_PlaylistSong] PRIMARY KEY ([PlaylistId], [SongId], [Index]),
    CONSTRAINT [FK_PlaylistSong_Playlists_PlaylistId] FOREIGN KEY ([PlaylistId]) REFERENCES [Playlists] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlaylistSong_Subjects_SongId] FOREIGN KEY ([SongId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Playlists_UserId] ON [Playlists] ([UserId]);

GO

CREATE INDEX [IX_PlaylistSong_SongId] ON [PlaylistSong] ([SongId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200301214742_AddPlaylist', N'3.1.1');

GO

