ALTER TABLE [MediumTypes] ADD [Visible] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200125111905_AddMediumTypeVisible', N'3.1.1');

GO

