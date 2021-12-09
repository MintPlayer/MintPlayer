CREATE TABLE [Media] (
    [Id] int NOT NULL IDENTITY,
    [TypeId] int NULL,
    [SubjectId] int NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_Media] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Media_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Media_MediumTypes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [MediumTypes] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Media_SubjectId] ON [Media] ([SubjectId]);

GO

CREATE INDEX [IX_Media_TypeId] ON [Media] ([TypeId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20191227195004_AddMedia', N'3.1.0');

GO

