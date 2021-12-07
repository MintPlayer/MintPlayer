BEGIN TRANSACTION;
GO

ALTER TABLE [ArtistPerson] DROP CONSTRAINT [FK_ArtistPerson_Subjects_ArtistId];
GO

ALTER TABLE [ArtistPerson] DROP CONSTRAINT [FK_ArtistPerson_Subjects_PersonId];
GO

ALTER TABLE [ArtistSong] DROP CONSTRAINT [FK_ArtistSong_Subjects_ArtistId];
GO

ALTER TABLE [ArtistSong] DROP CONSTRAINT [FK_ArtistSong_Subjects_SongId];
GO

ALTER TABLE [Lyrics] DROP CONSTRAINT [FK_Lyrics_Subjects_SongId];
GO

ALTER TABLE [PlaylistSong] DROP CONSTRAINT [FK_PlaylistSong_Subjects_SongId];
GO

CREATE TABLE [Artists] (
    [Id] int NOT NULL,
    [Name] nvarchar(max) NULL,
    [YearStarted] int NULL,
    [YearQuit] int NULL,
    CONSTRAINT [PK_Artists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Artists_Subjects_Id] FOREIGN KEY ([Id]) REFERENCES [Subjects] ([Id])
);
GO

CREATE TABLE [People] (
    [Id] int NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [Born] datetime2 NULL,
    [Died] datetime2 NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_People_Subjects_Id] FOREIGN KEY ([Id]) REFERENCES [Subjects] ([Id])
);
GO

CREATE TABLE [Songs] (
    [Id] int NOT NULL,
    [Title] nvarchar(max) NULL,
    [Released] datetime2 NOT NULL,
    CONSTRAINT [PK_Songs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Songs_Subjects_Id] FOREIGN KEY ([Id]) REFERENCES [Subjects] ([Id])
);
GO

INSERT INTO [People] (Id, FirstName, LastName, Born, Died)
				SELECT Id, FirstName, LastName, Born, Died
				FROM [Subjects]
				WHERE [Subjects].[SubjectType] = 'person'
GO

INSERT INTO [Artists] (Id, Name, YearStarted, YearQuit)
				SELECT Id, Name, YearStarted, YearQuit
				FROM [Subjects]
				WHERE [Subjects].[SubjectType] = 'artist'
GO

INSERT INTO [Songs] (Id, Title, Released)
				SELECT Id, Title, Released
				FROM [Subjects]
				WHERE [Subjects].[SubjectType] = 'song'
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Born');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Subjects] DROP COLUMN [Born];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Died');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Subjects] DROP COLUMN [Died];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'FirstName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Subjects] DROP COLUMN [FirstName];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'LastName');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Subjects] DROP COLUMN [LastName];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Name');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Subjects] DROP COLUMN [Name];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Released');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Subjects] DROP COLUMN [Released];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'SubjectType');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Subjects] DROP COLUMN [SubjectType];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'Title');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Subjects] DROP COLUMN [Title];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'YearQuit');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [Subjects] DROP COLUMN [YearQuit];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'YearStarted');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Subjects] DROP COLUMN [YearStarted];
GO

ALTER TABLE [ArtistPerson] ADD CONSTRAINT [FK_ArtistPerson_Artists_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Artists] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ArtistPerson] ADD CONSTRAINT [FK_ArtistPerson_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ArtistSong] ADD CONSTRAINT [FK_ArtistSong_Artists_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Artists] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ArtistSong] ADD CONSTRAINT [FK_ArtistSong_Songs_SongId] FOREIGN KEY ([SongId]) REFERENCES [Songs] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Lyrics] ADD CONSTRAINT [FK_Lyrics_Songs_SongId] FOREIGN KEY ([SongId]) REFERENCES [Songs] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [PlaylistSong] ADD CONSTRAINT [FK_PlaylistSong_Songs_SongId] FOREIGN KEY ([SongId]) REFERENCES [Songs] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211206134208_UseTptMapping', N'6.0.0');
GO

COMMIT;
GO

