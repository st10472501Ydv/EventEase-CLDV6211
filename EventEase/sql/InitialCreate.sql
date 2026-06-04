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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE TABLE [EventTypes] (
        [EventTypeId] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_EventTypes] PRIMARY KEY ([EventTypeId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE TABLE [Venues] (
        [VenueId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [Capacity] int NOT NULL,
        [ImageUrl] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Venues] PRIMARY KEY ([VenueId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE TABLE [Events] (
        [EventId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [OrganizerName] nvarchar(max) NULL,
        [OrganizerEmail] nvarchar(max) NULL,
        [EventTypeId] int NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([EventId]),
        CONSTRAINT [FK_Events_EventTypes_EventTypeId] FOREIGN KEY ([EventTypeId]) REFERENCES [EventTypes] ([EventTypeId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [BookingId] int NOT NULL IDENTITY,
        [VenueId] int NOT NULL,
        [EventId] int NOT NULL,
        [BookingDate] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
        CONSTRAINT [FK_Bookings_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([VenueId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EventTypeId', N'Name') AND [object_id] = OBJECT_ID(N'[EventTypes]'))
        SET IDENTITY_INSERT [EventTypes] ON;
    EXEC(N'INSERT INTO [EventTypes] ([EventTypeId], [Name])
    VALUES (1, N''Conference''),
    (2, N''Workshop''),
    (3, N''Wedding''),
    (4, N''Party''),
    (5, N''Seminar''),
    (6, N''Concert''),
    (7, N''Exhibition''),
    (8, N''Other'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EventTypeId', N'Name') AND [object_id] = OBJECT_ID(N'[EventTypes]'))
        SET IDENTITY_INSERT [EventTypes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bookings_EventId] ON [Bookings] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_VenueId] ON [Bookings] ([VenueId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Events_EventTypeId] ON [Events] ([EventTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604151134_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260604151134_InitialCreate', N'10.0.6');
END;

COMMIT;
GO

