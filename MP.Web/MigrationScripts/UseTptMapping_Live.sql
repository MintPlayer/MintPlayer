BEGIN TRANSACTION;
GO

ALTER TABLE [mintplay].[ArtistPerson] DROP CONSTRAINT [FK_ArtistPerson_Subjects_ArtistId];
GO

ALTER TABLE [mintplay].[ArtistPerson] DROP CONSTRAINT [FK_ArtistPerson_Subjects_PersonId];
GO

ALTER TABLE [mintplay].[ArtistSong] DROP CONSTRAINT [FK_ArtistSong_Subjects_ArtistId];
GO

ALTER TABLE [mintplay].[ArtistSong] DROP CONSTRAINT [FK_ArtistSong_Subjects_SongId];
GO

ALTER TABLE [mintplay].[Lyrics] DROP CONSTRAINT [FK_Lyrics_Subjects_SongId];
GO

ALTER TABLE [mintplay].[PlaylistSong] DROP CONSTRAINT [FK_PlaylistSong_Subjects_SongId];
GO

CREATE TABLE [mintplay].[Artists] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NULL,
    [YearStarted] int NULL,
    [YearQuit] int NULL,
    CONSTRAINT [PK_Artists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Artists_Subjects_Id] FOREIGN KEY ([Id]) REFERENCES [mintplay].[Subjects] ([Id])
);
GO

CREATE TABLE [mintplay].[People] (
    [Id] int NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [Born] datetime2 NULL,
    [Died] datetime2 NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_People_Subjects_Id] FOREIGN KEY ([Id]) REFERENCES [mintplay].[Subjects] ([Id])
);
GO

CREATE TABLE [mintplay].[Songs] (
    [Id] int NOT NULL,
    [Title] nvarchar(max) NULL,
    [Released] datetime2 NOT NULL,
    CONSTRAINT [PK_Songs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Songs_Subjects_Id] FOREIGN KEY ([Id]) REFERENCES [mintplay].[Subjects] ([Id])
);
GO

INSERT INTO [mintplay].[People] (Id, FirstName, LastName, Born, Died)
				SELECT Id, FirstName, LastName, Born, Died
				FROM [mintplay].[Subjects]
				WHERE [Subjects].[SubjectType] = 'person'
GO

INSERT INTO [mintplay].[Artists] (Id, Name, YearStarted, YearQuit)
				SELECT Id, Name, YearStarted, YearQuit
				FROM [mintplay].[Subjects]
				WHERE [Subjects].[SubjectType] = 'artist'
GO

INSERT INTO [mintplay].[Songs] (Id, Title, Released)
				SELECT Id, Title, Released
				FROM [mintplay].[Subjects]
				WHERE [Subjects].[SubjectType] = 'song'
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Born');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [Born];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Died');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [Died];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'FirstName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [FirstName];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'LastName');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [LastName];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Name');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [Name];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Released');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [Released];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'SubjectType');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [SubjectType];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Title');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [Title];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'YearQuit');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [YearQuit];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'YearStarted');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [mintplay].[Subjects] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [mintplay].[Subjects] DROP COLUMN [YearStarted];
GO

ALTER TABLE [mintplay].[ArtistPerson] ADD CONSTRAINT [FK_ArtistPerson_Artists_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [mintplay].[Artists] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [mintplay].[ArtistPerson] ADD CONSTRAINT [FK_ArtistPerson_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [mintplay].[People] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [mintplay].[ArtistSong] ADD CONSTRAINT [FK_ArtistSong_Artists_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [mintplay].[Artists] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [mintplay].[ArtistSong] ADD CONSTRAINT [FK_ArtistSong_Songs_SongId] FOREIGN KEY ([SongId]) REFERENCES [mintplay].[Songs] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [mintplay].[Lyrics] ADD CONSTRAINT [FK_Lyrics_Songs_SongId] FOREIGN KEY ([SongId]) REFERENCES [mintplay].[Songs] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [mintplay].[PlaylistSong] ADD CONSTRAINT [FK_PlaylistSong_Songs_SongId] FOREIGN KEY ([SongId]) REFERENCES [mintplay].[Songs] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [mintplay].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211206134208_UseTptMapping', N'6.0.0');
GO

COMMIT;
GO

