-- =============================================
-- Time Tracking Feature - Database Migration
-- Add TimeCategories and TimeEntries tables
-- =============================================

USE [MySuperSystem2025]
GO

-- Create TimeCategories table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TimeCategories]') AND type in (N'U'))
BEGIN
    SET QUOTED_IDENTIFIER ON
    
    CREATE TABLE [dbo].[TimeCategories](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2(7) NULL,
        CONSTRAINT [PK_TimeCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TimeCategories_AspNetUsers] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    PRINT 'TimeCategories table created successfully'
END
ELSE
BEGIN
    PRINT 'TimeCategories table already exists'
END
GO

-- Create filtered index on TimeCategories
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TimeCategories_UserId_Name' AND object_id = OBJECT_ID('TimeCategories'))
BEGIN
    SET QUOTED_IDENTIFIER ON
    CREATE UNIQUE INDEX [IX_TimeCategories_UserId_Name] ON [dbo].[TimeCategories]([UserId], [Name]) WHERE [IsDeleted] = 0
    PRINT 'TimeCategories index created successfully'
END
GO

-- Create TimeEntries table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TimeEntries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TimeEntries](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StartTime] DATETIME2(7) NOT NULL,
        [EndTime] DATETIME2(7) NOT NULL,
        [DurationMinutes] INT NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [CategoryId] INT NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2(7) NULL,
        CONSTRAINT [PK_TimeEntries] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_TimeEntries_TimeCategories] FOREIGN KEY([CategoryId]) REFERENCES [dbo].[TimeCategories] ([Id]),
        CONSTRAINT [FK_TimeEntries_AspNetUsers] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    CREATE INDEX [IX_TimeEntries_StartTime] ON [dbo].[TimeEntries]([StartTime])
    CREATE INDEX [IX_TimeEntries_UserId] ON [dbo].[TimeEntries]([UserId])
    CREATE INDEX [IX_TimeEntries_CategoryId] ON [dbo].[TimeEntries]([CategoryId])
    
    PRINT 'TimeEntries table created successfully'
END
ELSE
BEGIN
    PRINT 'TimeEntries table already exists'
END
GO

PRINT 'Time Tracking migration completed successfully!'
GO
