ALTER TABLE [Subjects] ADD [ConcurrencyStamp] rowversion NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210815222531_AddConcurrencyStamps', N'3.1.17');

GO

