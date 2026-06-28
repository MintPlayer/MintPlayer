IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [PictureUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(50) NOT NULL,
        [ProviderKey] nvarchar(200) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(50) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227095433_AddIdentity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20191227095433_AddIdentity', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE TABLE [ArtistPerson] (
        [ArtistId] int NOT NULL,
        [PersonId] int NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_ArtistPerson] PRIMARY KEY ([ArtistId], [PersonId]),
        CONSTRAINT [FK_ArtistPerson_Subjects_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ArtistPerson_Subjects_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE TABLE [ArtistSong] (
        [ArtistId] int NOT NULL,
        [SongId] int NOT NULL,
        CONSTRAINT [PK_ArtistSong] PRIMARY KEY ([ArtistId], [SongId]),
        CONSTRAINT [FK_ArtistSong_Subjects_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ArtistSong_Subjects_SongId] FOREIGN KEY ([SongId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE TABLE [Lyrics] (
        [SongId] int NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Text] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Lyrics] PRIMARY KEY ([SongId], [UserId]),
        CONSTRAINT [FK_Lyrics_Subjects_SongId] FOREIGN KEY ([SongId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Lyrics_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE INDEX [IX_ArtistPerson_PersonId] ON [ArtistPerson] ([PersonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE INDEX [IX_ArtistSong_SongId] ON [ArtistSong] ([SongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE INDEX [IX_Lyrics_UserId] ON [Lyrics] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE INDEX [IX_Subjects_UserDeleteId] ON [Subjects] ([UserDeleteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE INDEX [IX_Subjects_UserInsertId] ON [Subjects] ([UserInsertId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    CREATE INDEX [IX_Subjects_UserUpdateId] ON [Subjects] ([UserUpdateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227135150_AddCoreEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20191227135150_AddCoreEntities', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227192024_AddLikes'
)
BEGIN
    CREATE TABLE [Likes] (
        [SubjectId] int NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [DoesLike] bit NOT NULL,
        CONSTRAINT [PK_Likes] PRIMARY KEY ([SubjectId], [UserId]),
        CONSTRAINT [FK_Likes_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Likes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227192024_AddLikes'
)
BEGIN
    CREATE INDEX [IX_Likes_UserId] ON [Likes] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227192024_AddLikes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20191227192024_AddLikes', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227193309_AddMediumTypes'
)
BEGIN
    CREATE TABLE [MediumTypes] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(max) NULL,
        [PlayerType] int NOT NULL,
        [UserInsertId] uniqueidentifier NULL,
        [UserUpdateId] uniqueidentifier NULL,
        [UserDeleteId] uniqueidentifier NULL,
        CONSTRAINT [PK_MediumTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MediumTypes_AspNetUsers_UserDeleteId] FOREIGN KEY ([UserDeleteId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MediumTypes_AspNetUsers_UserInsertId] FOREIGN KEY ([UserInsertId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MediumTypes_AspNetUsers_UserUpdateId] FOREIGN KEY ([UserUpdateId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227193309_AddMediumTypes'
)
BEGIN
    CREATE INDEX [IX_MediumTypes_UserDeleteId] ON [MediumTypes] ([UserDeleteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227193309_AddMediumTypes'
)
BEGIN
    CREATE INDEX [IX_MediumTypes_UserInsertId] ON [MediumTypes] ([UserInsertId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227193309_AddMediumTypes'
)
BEGIN
    CREATE INDEX [IX_MediumTypes_UserUpdateId] ON [MediumTypes] ([UserUpdateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227193309_AddMediumTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20191227193309_AddMediumTypes', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227195004_AddMedia'
)
BEGIN
    CREATE TABLE [Media] (
        [Id] int NOT NULL IDENTITY,
        [TypeId] int NULL,
        [SubjectId] int NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_Media] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Media_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Media_MediumTypes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [MediumTypes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227195004_AddMedia'
)
BEGIN
    CREATE INDEX [IX_Media_SubjectId] ON [Media] ([SubjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227195004_AddMedia'
)
BEGIN
    CREATE INDEX [IX_Media_TypeId] ON [Media] ([TypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191227195004_AddMedia'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20191227195004_AddMedia', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191228123703_AddJobs'
)
BEGIN
    CREATE TABLE [Jobs] (
        [Id] int NOT NULL IDENTITY,
        [Status] int NOT NULL,
        [JobType] nvarchar(max) NOT NULL,
        [SubjectId] int NULL,
        [SubjectStatus] int NULL,
        CONSTRAINT [PK_Jobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Jobs_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191228123703_AddJobs'
)
BEGIN
    CREATE INDEX [IX_Jobs_SubjectId] ON [Jobs] ([SubjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20191228123703_AddJobs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20191228123703_AddJobs', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200113130019_AddTimestamps'
)
BEGIN
    ALTER TABLE [Subjects] ADD [DateDelete] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200113130019_AddTimestamps'
)
BEGIN
    ALTER TABLE [Subjects] ADD [DateInsert] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200113130019_AddTimestamps'
)
BEGIN
    ALTER TABLE [Subjects] ADD [DateUpdate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200113130019_AddTimestamps'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200113130019_AddTimestamps', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200125111905_AddMediumTypeVisible'
)
BEGIN
    ALTER TABLE [MediumTypes] ADD [Visible] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200125111905_AddMediumTypeVisible'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200125111905_AddMediumTypeVisible', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE TABLE [TagCategories] (
        [Id] int NOT NULL IDENTITY,
        [Color] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [UserInsertId] uniqueidentifier NULL,
        [UserUpdateId] uniqueidentifier NULL,
        [UserDeleteId] uniqueidentifier NULL,
        [DateInsert] datetime2 NOT NULL,
        [DateUpdate] datetime2 NULL,
        [DateDelete] datetime2 NULL,
        CONSTRAINT [PK_TagCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TagCategories_AspNetUsers_UserDeleteId] FOREIGN KEY ([UserDeleteId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TagCategories_AspNetUsers_UserInsertId] FOREIGN KEY ([UserInsertId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TagCategories_AspNetUsers_UserUpdateId] FOREIGN KEY ([UserUpdateId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE TABLE [Tags] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(max) NULL,
        [CategoryId] int NULL,
        [UserInsertId] uniqueidentifier NULL,
        [UserUpdateId] uniqueidentifier NULL,
        [UserDeleteId] uniqueidentifier NULL,
        [DateInsert] datetime2 NOT NULL,
        [DateUpdate] datetime2 NULL,
        [DateDelete] datetime2 NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tags_TagCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [TagCategories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tags_AspNetUsers_UserDeleteId] FOREIGN KEY ([UserDeleteId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tags_AspNetUsers_UserInsertId] FOREIGN KEY ([UserInsertId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tags_AspNetUsers_UserUpdateId] FOREIGN KEY ([UserUpdateId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE TABLE [SubjectTag] (
        [SubjectId] int NOT NULL,
        [TagId] int NOT NULL,
        CONSTRAINT [PK_SubjectTag] PRIMARY KEY ([SubjectId], [TagId]),
        CONSTRAINT [FK_SubjectTag_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SubjectTag_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_SubjectTag_TagId] ON [SubjectTag] ([TagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_TagCategories_UserDeleteId] ON [TagCategories] ([UserDeleteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_TagCategories_UserInsertId] ON [TagCategories] ([UserInsertId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_TagCategories_UserUpdateId] ON [TagCategories] ([UserUpdateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_Tags_CategoryId] ON [Tags] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_Tags_UserDeleteId] ON [Tags] ([UserDeleteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_Tags_UserInsertId] ON [Tags] ([UserInsertId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    CREATE INDEX [IX_Tags_UserUpdateId] ON [Tags] ([UserUpdateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200206120201_AddTags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200206120201_AddTags', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200301214742_AddPlaylist'
)
BEGIN
    CREATE TABLE [Playlists] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NULL,
        [Description] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Playlists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Playlists_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200301214742_AddPlaylist'
)
BEGIN
    CREATE TABLE [PlaylistSong] (
        [PlaylistId] int NOT NULL,
        [SongId] int NOT NULL,
        [Index] int NOT NULL,
        CONSTRAINT [PK_PlaylistSong] PRIMARY KEY ([PlaylistId], [SongId], [Index]),
        CONSTRAINT [FK_PlaylistSong_Playlists_PlaylistId] FOREIGN KEY ([PlaylistId]) REFERENCES [Playlists] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PlaylistSong_Subjects_SongId] FOREIGN KEY ([SongId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200301214742_AddPlaylist'
)
BEGIN
    CREATE INDEX [IX_Playlists_UserId] ON [Playlists] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200301214742_AddPlaylist'
)
BEGIN
    CREATE INDEX [IX_PlaylistSong_SongId] ON [PlaylistSong] ([SongId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200301214742_AddPlaylist'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200301214742_AddPlaylist', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200327151033_AddSubtags'
)
BEGIN
    ALTER TABLE [Tags] ADD [ParentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200327151033_AddSubtags'
)
BEGIN
    CREATE INDEX [IX_Tags_ParentId] ON [Tags] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200327151033_AddSubtags'
)
BEGIN
    ALTER TABLE [Tags] ADD CONSTRAINT [FK_Tags_Tags_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Tags] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200327151033_AddSubtags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200327151033_AddSubtags', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200331091735_AddLyricsTimeline'
)
BEGIN
    ALTER TABLE [Lyrics] ADD [Timeline] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200331091735_AddLyricsTimeline'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200331091735_AddLyricsTimeline', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506115211_SeedRoles'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (''93c9bda5-8254-486f-ade1-95b5b66e83db'', NULL, N''Blogger'', N''Blogger'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506115211_SeedRoles'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (''91f3cec8-a67d-45f3-b718-22cf71961b05'', NULL, N''Administrator'', N''Administrator'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506115211_SeedRoles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200506115211_SeedRoles', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506133907_AddBlogPosts'
)
BEGIN
    CREATE TABLE [BlogPosts] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NULL,
        [Headline] nvarchar(max) NULL,
        [Body] nvarchar(max) NULL,
        [UserInsertId] uniqueidentifier NULL,
        [UserUpdateId] uniqueidentifier NULL,
        [UserDeleteId] uniqueidentifier NULL,
        [DateInsert] datetime2 NOT NULL,
        [DateUpdate] datetime2 NULL,
        [DateDelete] datetime2 NULL,
        CONSTRAINT [PK_BlogPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BlogPosts_AspNetUsers_UserDeleteId] FOREIGN KEY ([UserDeleteId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BlogPosts_AspNetUsers_UserInsertId] FOREIGN KEY ([UserInsertId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BlogPosts_AspNetUsers_UserUpdateId] FOREIGN KEY ([UserUpdateId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506133907_AddBlogPosts'
)
BEGIN
    CREATE INDEX [IX_BlogPosts_UserDeleteId] ON [BlogPosts] ([UserDeleteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506133907_AddBlogPosts'
)
BEGIN
    CREATE INDEX [IX_BlogPosts_UserInsertId] ON [BlogPosts] ([UserInsertId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506133907_AddBlogPosts'
)
BEGIN
    CREATE INDEX [IX_BlogPosts_UserUpdateId] ON [BlogPosts] ([UserUpdateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20200506133907_AddBlogPosts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200506133907_AddBlogPosts', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210111223453_AddPlaylistAccessibility'
)
BEGIN
    ALTER TABLE [Playlists] ADD [Accessibility] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210111223453_AddPlaylistAccessibility'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210111223453_AddPlaylistAccessibility', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210225090127_AddLogEntries'
)
BEGIN
    CREATE TABLE [LogEntries] (
        [Id] int NOT NULL IDENTITY,
        [Text] nvarchar(max) NULL,
        CONSTRAINT [PK_LogEntries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210225090127_AddLogEntries'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210225090127_AddLogEntries', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210225175302_AddUncreditedArtists'
)
BEGIN
    ALTER TABLE [ArtistSong] ADD [Credited] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210225175302_AddUncreditedArtists'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210225175302_AddUncreditedArtists', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210721151845_Bypass2faForExternalLogin'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Bypass2faForExternalLogin] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210721151845_Bypass2faForExternalLogin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210721151845_Bypass2faForExternalLogin', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210812100810_RemovedPlayerType'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MediumTypes]') AND [c].[name] = N'PlayerType');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [MediumTypes] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [MediumTypes] DROP COLUMN [PlayerType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210812100810_RemovedPlayerType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210812100810_RemovedPlayerType', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210815222531_AddConcurrencyStamps'
)
BEGIN
    ALTER TABLE [Subjects] ADD [ConcurrencyStamp] rowversion NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20210815222531_AddConcurrencyStamps'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210815222531_AddConcurrencyStamps', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218072728_UpgradeToNet10'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subjects]') AND [c].[name] = N'SubjectType');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Subjects] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Subjects] ALTER COLUMN [SubjectType] nvarchar(8) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218072728_UpgradeToNet10'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Jobs]') AND [c].[name] = N'JobType');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Jobs] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Jobs] ALTER COLUMN [JobType] nvarchar(13) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218072728_UpgradeToNet10'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251218072728_UpgradeToNet10', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218075358_AddWebAuthnCredentials'
)
BEGIN
    CREATE TABLE [WebAuthnCredentials] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [CredentialId] varbinary(900) NULL,
        [PublicKey] varbinary(max) NULL,
        [UserHandle] varbinary(max) NULL,
        [SignatureCounter] bigint NOT NULL,
        [CredType] nvarchar(max) NULL,
        [RegDate] datetime2 NOT NULL,
        [AaGuid] uniqueidentifier NOT NULL,
        [DisplayName] nvarchar(max) NULL,
        [LastUsed] datetime2 NULL,
        CONSTRAINT [PK_WebAuthnCredentials] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WebAuthnCredentials_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218075358_AddWebAuthnCredentials'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_WebAuthnCredentials_CredentialId] ON [WebAuthnCredentials] ([CredentialId]) WHERE [CredentialId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218075358_AddWebAuthnCredentials'
)
BEGIN
    CREATE INDEX [IX_WebAuthnCredentials_UserId] ON [WebAuthnCredentials] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251218075358_AddWebAuthnCredentials'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251218075358_AddWebAuthnCredentials', N'10.0.0');
END;

COMMIT;
GO

