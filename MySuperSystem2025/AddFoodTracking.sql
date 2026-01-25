-- =============================================
-- Food Tracking Tables Migration Script
-- Run this script to add Food Tracking support
-- =============================================

USE [MySuperSystem2025]
GO

-- Create FoodEntries table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoodEntries')
BEGIN
    CREATE TABLE [dbo].[FoodEntries] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [MealType] NVARCHAR(50) NOT NULL,
        [Date] DATETIME2(7) NOT NULL,
        [ServingSize] NVARCHAR(100) NULL,
        [Calories] INT NOT NULL DEFAULT 0,
        [Protein] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Carbs] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Fats] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Notes] NVARCHAR(500) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2(7) NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2(7) NULL,
        CONSTRAINT [PK_FoodEntries] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_FoodEntries_AspNetUsers_UserId] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    );

    -- Create indexes for better query performance
    CREATE NONCLUSTERED INDEX [IX_FoodEntries_UserId] ON [dbo].[FoodEntries] ([UserId]);
    CREATE NONCLUSTERED INDEX [IX_FoodEntries_Date] ON [dbo].[FoodEntries] ([Date]);
    CREATE NONCLUSTERED INDEX [IX_FoodEntries_MealType] ON [dbo].[FoodEntries] ([MealType]);

    PRINT 'FoodEntries table created successfully!';
END
ELSE
BEGIN
    PRINT 'FoodEntries table already exists.';
END
GO

-- Verify the table was created
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.name = 'FoodEntries'
ORDER BY c.column_id;
GO

PRINT '';
PRINT '=============================================';
PRINT 'Food Tracking Migration Complete!';
PRINT '=============================================';
PRINT '';
PRINT 'The FoodEntries table has been created.';
PRINT 'You can now use the Food Tracker feature.';
PRINT '';
GO
