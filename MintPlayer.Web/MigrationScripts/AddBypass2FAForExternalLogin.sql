ALTER TABLE [AspNetUsers] ADD [Bypass2faForExternalLogin] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210721151845_Bypass2faForExternalLogin', N'3.1.17');

GO

