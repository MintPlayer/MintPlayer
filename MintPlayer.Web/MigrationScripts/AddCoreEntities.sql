CREATE TABLE [Subjects] (
    [Id] int NOT NULL IDENTITY,
    [UserInsertId] uniqueidentifier NULL,
    [UserUpdateId] uniqueidentifier NULL,
    [UserDeleteId] uniqueidentifier NULL,
    [SubjectType] nvarchar(max) NOT NULL,
    [Name] nvarchar(max) NULL,
    [YearStarted] int NULL,
    [YearQuit] int NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [Born] datetime2 NULL,
    [Died] datetime2 NULL,
    [Title] nvarchar(max) NULL,
    [Released] datetime2 NULL,
    CONSTRAINT [PK_Subjects] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Subjects_AspNetUsers_UserDeleteId] FOREIGN KEY ([UserDeleteId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Subjects_AspNetUsers_UserInsertId] FOREIGN KEY ([UserInsertId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Subjects_AspNetUsers_UserUpdateId] FOREIGN KEY ([UserUpdateId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [ArtistPerson] (
    [ArtistId] int NOT NULL,
    [PersonId] int NOT NULL,
    [Active] bit NOT NULL,
    CONSTRAINT [PK_ArtistPerson] PRIMARY KEY ([ArtistId], [PersonId]),
    CONSTRAINT [FK_ArtistPerson_Subjects_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ArtistPerson_Subjects_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [ArtistSong] (
    [ArtistId] int NOT NULL,
    [SongId] int NOT NULL,
    CONSTRAINT [PK_ArtistSong] PRIMARY KEY ([ArtistId], [SongId]),
    CONSTRAINT [FK_ArtistSong_Subjects_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ArtistSong_Subjects_SongId] FOREIGN KEY ([SongId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Lyrics] (
    [SongId] int NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Text] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Lyrics] PRIMARY KEY ([SongId], [UserId]),
    CONSTRAINT [FK_Lyrics_Subjects_SongId] FOREIGN KEY ([SongId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Lyrics_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_ArtistPerson_PersonId] ON [ArtistPerson] ([PersonId]);

GO

CREATE INDEX [IX_ArtistSong_SongId] ON [ArtistSong] ([SongId]);

GO

CREATE INDEX [IX_Lyrics_UserId] ON [Lyrics] ([UserId]);

GO

CREATE INDEX [IX_Subjects_UserDeleteId] ON [Subjects] ([UserDeleteId]);

GO

CREATE INDEX [IX_Subjects_UserInsertId] ON [Subjects] ([UserInsertId]);

GO

CREATE INDEX [IX_Subjects_UserUpdateId] ON [Subjects] ([UserUpdateId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20191227135150_AddCoreEntities', N'3.1.0');

GO

