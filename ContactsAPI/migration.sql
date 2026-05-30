IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Contacts] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ExtraFieldDefinitions] (
    [ExtraFieldDefinitionId] int NOT NULL IDENTITY,
    [FieldName] nvarchar(max) NOT NULL,
    [FieldType] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [AllowedRoles] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ExtraFieldDefinitions] PRIMARY KEY ([ExtraFieldDefinitionId])
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [ContactExtraFields] (
    [ExtraFieldId] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ExtraFieldDefinitionId] int NOT NULL,
    [FieldValue] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ContactExtraFields] PRIMARY KEY ([ExtraFieldId]),
    CONSTRAINT [FK_ContactExtraFields_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContactExtraFields_ExtraFieldDefinitions_ExtraFieldDefinitionId] FOREIGN KEY ([ExtraFieldDefinitionId]) REFERENCES [ExtraFieldDefinitions] ([ExtraFieldDefinitionId]) ON DELETE CASCADE
);
GO

CREATE TABLE [AuditLogs] (
    [AuditLogId] int NOT NULL IDENTITY,
    [Timestamp] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [UserRole] nvarchar(max) NOT NULL,
    [ActionType] nvarchar(max) NOT NULL,
    [EntityName] nvarchar(max) NOT NULL,
    [EntityId] nvarchar(max) NOT NULL,
    [RequestData] nvarchar(max) NOT NULL,
    [ResponseData] nvarchar(max) NOT NULL,
    [Success] bit NOT NULL,
    [ErrorMessage] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogId]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
GO

CREATE INDEX [IX_ContactExtraFields_ContactId] ON [ContactExtraFields] ([ContactId]);
GO

CREATE INDEX [IX_ContactExtraFields_ExtraFieldDefinitionId] ON [ContactExtraFields] ([ExtraFieldDefinitionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260122014910_Initial', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'Email', N'PasswordHash', N'Role', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [Email], [PasswordHash], [Role], [Username])
VALUES (1, N'admin@example.com', N'$2a$11$6QR6oAAIBpD9Z.Z7xEXKk.pzK275JRyImXkvEA8.MaqjKKshKowyW', N'Admin', N'admin');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'Email', N'PasswordHash', N'Role', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260125223654_SeedAdminUser', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AuditLogs] DROP CONSTRAINT [FK_AuditLogs_Users_UserId];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'UserId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [AuditLogs] ALTER COLUMN [UserId] int NULL;
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$bgszE9CQ4dl2dDsw2oymVe9fubexsPTlUP02mDeE14exjyYBTJupq'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

ALTER TABLE [AuditLogs] ADD CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260126055354_MakeAuditLogUserIdNullable', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ExtraFieldDefinitions]') AND [c].[name] = N'AllowedRoles');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ExtraFieldDefinitions] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ExtraFieldDefinitions] DROP COLUMN [AllowedRoles];
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$Vp9wnpW5ug45spNHi2E4iu2I3.Wtm3CCay7QpKT61a5YHlUURxiEq'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260202025305_RemoveAllowedRolesFromExtraFieldDefinition', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AdminConfigs] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(max) NOT NULL,
    [Value] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_AdminConfigs] PRIMARY KEY ([Id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Value') AND [object_id] = OBJECT_ID(N'[AdminConfigs]'))
    SET IDENTITY_INSERT [AdminConfigs] ON;
INSERT INTO [AdminConfigs] ([Id], [Description], [Key], [Value])
VALUES (1, N'Toggle audit logging on/off', N'EnableAuditLogging', N'true'),
(2, N'Maximum number of extra fields allowed per contact', N'MaxExtraFieldsPerContact', N'5');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Value') AND [object_id] = OBJECT_ID(N'[AdminConfigs]'))
    SET IDENTITY_INSERT [AdminConfigs] OFF;
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$eh81bGdoBG.graUR282wEuA4k7crvl40hJl2KKXB/XPhDm0xBqvGi'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260202110343_AddAdminConfigs', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [Expires] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
);
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$7WGKr8YjIpPitHkzR9z9he22GZJK6vmQ6/SccxrR1Zd4VOxEPGJPS'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260209004102_AddRefreshTokenAndLogOut', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

ALTER TABLE [Users] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Contacts] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

ALTER TABLE [Contacts] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'';
GO

UPDATE [Users] SET [CreatedDate] = '2026-04-30T00:00:00.0000000Z', [PasswordHash] = N'$2a$11$8zdOxeqWY8SgT5CZAganQ.rKvbP4NcmrB6oBzrSKkxLjfHUtY57Ei', [Status] = N'Active'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260430015237_AddStatusAndCreatedDate', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ExtraFieldDefinitions] ADD [IsRequired] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$prL8DbPLX4xA2S6nqugKhOBckMGxN5.imWDm24.MZNH7ejj3cWL/q'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260517034723_AddIsRequiredToExtraFieldDefinition', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                UPDATE ExtraFieldDefinitions
                SET FieldType = UPPER(LEFT(FieldType,1)) + LOWER(SUBSTRING(FieldType,2,LEN(FieldType)))
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$Rs9L2KSNdYQAnqywojQB4.F.zfcHU2hrA0oN4XMjma4kVBjxrbH2a'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260517225056_ConvertFieldTypeToEnum', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ExtraFieldOptions] (
    [ExtraFieldOptionId] int NOT NULL IDENTITY,
    [ExtraFieldDefinitionId] int NOT NULL,
    [OptionValue] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_ExtraFieldOptions] PRIMARY KEY ([ExtraFieldOptionId]),
    CONSTRAINT [FK_ExtraFieldOptions_ExtraFieldDefinitions_ExtraFieldDefinitionId] FOREIGN KEY ([ExtraFieldDefinitionId]) REFERENCES [ExtraFieldDefinitions] ([ExtraFieldDefinitionId]) ON DELETE CASCADE
);
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$axTR4GIap2fi3UM/9yzste0qmRzU3/9DUfMEfP5bJnHfkr7nm9.Ru'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

CREATE INDEX [IX_ExtraFieldOptions_ExtraFieldDefinitionId] ON [ExtraFieldOptions] ([ExtraFieldDefinitionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260517231129_AddExtraFieldOptions', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AuditLogs] ADD [PerformedByUsername] nvarchar(max) NOT NULL DEFAULT N'Anonymous';
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$K0PFLg0DcErkpkJS6QGObuhOTvpjGDZl5TDlMQOqmIREMTnMvkBte'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260524044623_AddPerformedByUsernameToAuditLog', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

UPDATE [Users] SET [PasswordHash] = N'$2a$11$6QR6oAAIBpD9Z.Z7xEXKk.pzK275JRyImXkvEA8.MaqjKKshKowyW'
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedDate', N'Email', N'PasswordHash', N'Role', N'Status', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [CreatedDate], [Email], [PasswordHash], [Role], [Status], [Username])
VALUES (2, '2026-05-28T00:00:00.0000000Z', N'editor@example.com', N'$2a$11$6QR6oAAIBpD9Z.Z7xEXKk.pzK275JRyImXkvEA8.MaqjKKshKowyW', N'Editor', N'Active', N'editor');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedDate', N'Email', N'PasswordHash', N'Role', N'Status', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260528033441_SeedEditorUser', N'8.0.0');
GO

COMMIT;
GO

