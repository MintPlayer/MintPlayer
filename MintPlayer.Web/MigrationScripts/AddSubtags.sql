ALTER TABLE [Tags] ADD [ParentId] int NULL;

GO

CREATE INDEX [IX_Tags_ParentId] ON [Tags] ([ParentId]);

GO

ALTER TABLE [Tags] ADD CONSTRAINT [FK_Tags_Tags_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Tags] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200327151033_AddSubtags', N'3.1.1');

GO

