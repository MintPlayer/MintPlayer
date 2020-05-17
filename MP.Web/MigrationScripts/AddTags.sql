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

GO

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

GO

CREATE TABLE [SubjectTag] (
    [SubjectId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_SubjectTag] PRIMARY KEY ([SubjectId], [TagId]),
    CONSTRAINT [FK_SubjectTag_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubjectTag_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_SubjectTag_TagId] ON [SubjectTag] ([TagId]);

GO

CREATE INDEX [IX_TagCategories_UserDeleteId] ON [TagCategories] ([UserDeleteId]);

GO

CREATE INDEX [IX_TagCategories_UserInsertId] ON [TagCategories] ([UserInsertId]);

GO

CREATE INDEX [IX_TagCategories_UserUpdateId] ON [TagCategories] ([UserUpdateId]);

GO

CREATE INDEX [IX_Tags_CategoryId] ON [Tags] ([CategoryId]);

GO

CREATE INDEX [IX_Tags_UserDeleteId] ON [Tags] ([UserDeleteId]);

GO

CREATE INDEX [IX_Tags_UserInsertId] ON [Tags] ([UserInsertId]);

GO

CREATE INDEX [IX_Tags_UserUpdateId] ON [Tags] ([UserUpdateId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200206120201_AddTags', N'3.1.1');

GO

