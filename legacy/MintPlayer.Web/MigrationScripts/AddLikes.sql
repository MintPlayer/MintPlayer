CREATE TABLE [Likes] (
    [SubjectId] int NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [DoesLike] bit NOT NULL,
    CONSTRAINT [PK_Likes] PRIMARY KEY ([SubjectId], [UserId]),
    CONSTRAINT [FK_Likes_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Likes_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Likes_UserId] ON [Likes] ([UserId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20191227192024_AddLikes', N'3.1.0');

GO

