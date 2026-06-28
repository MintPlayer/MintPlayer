CREATE TABLE [Jobs] (
    [Id] int NOT NULL IDENTITY,
    [Status] int NOT NULL,
    [JobType] nvarchar(max) NOT NULL,
    [SubjectId] int NULL,
    [SubjectStatus] int NULL,
    CONSTRAINT [PK_Jobs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Jobs_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Jobs_SubjectId] ON [Jobs] ([SubjectId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20191228123703_AddJobs', N'3.1.0');

GO

