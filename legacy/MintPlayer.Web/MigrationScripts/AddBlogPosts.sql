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

GO

CREATE INDEX [IX_BlogPosts_UserDeleteId] ON [BlogPosts] ([UserDeleteId]);

GO

CREATE INDEX [IX_BlogPosts_UserInsertId] ON [BlogPosts] ([UserInsertId]);

GO

CREATE INDEX [IX_BlogPosts_UserUpdateId] ON [BlogPosts] ([UserUpdateId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200506133907_AddBlogPosts', N'3.1.3');

GO

