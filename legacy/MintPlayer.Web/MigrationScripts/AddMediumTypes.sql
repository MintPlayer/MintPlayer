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

GO

CREATE INDEX [IX_MediumTypes_UserDeleteId] ON [MediumTypes] ([UserDeleteId]);

GO

CREATE INDEX [IX_MediumTypes_UserInsertId] ON [MediumTypes] ([UserInsertId]);

GO

CREATE INDEX [IX_MediumTypes_UserUpdateId] ON [MediumTypes] ([UserUpdateId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20191227193309_AddMediumTypes', N'3.1.0');

GO

