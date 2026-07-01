BEGIN TRANSACTION;
GO

ALTER TABLE [ExpenseCategories] ADD [IsBudgetActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ExpenseCategories] ADD [IsHidden] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ExpenseCategories] ADD [RolloverEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [FoodEntries] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [MealType] nvarchar(50) NOT NULL,
    [Date] datetime2 NOT NULL,
    [ServingSize] nvarchar(100) NULL,
    [Calories] int NOT NULL,
    [Protein] decimal(18,2) NOT NULL,
    [Carbs] decimal(18,2) NOT NULL,
    [Fats] decimal(18,2) NOT NULL,
    [Notes] nvarchar(500) NULL,
    [UserId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_FoodEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FoodEntries_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [MonthlyBudgetHistories] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [BaseBudget] decimal(18,2) NOT NULL,
    [RolloverAmount] decimal(18,2) NOT NULL,
    [IsBudgetActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_MonthlyBudgetHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MonthlyBudgetHistories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MonthlyBudgetHistories_ExpenseCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ExpenseCategories] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_FoodEntries_Date] ON [FoodEntries] ([Date]);
GO

CREATE INDEX [IX_FoodEntries_MealType] ON [FoodEntries] ([MealType]);
GO

CREATE INDEX [IX_FoodEntries_UserId] ON [FoodEntries] ([UserId]);
GO

CREATE INDEX [IX_MonthlyBudgetHistories_CategoryId] ON [MonthlyBudgetHistories] ([CategoryId]);
GO

CREATE UNIQUE INDEX [IX_MonthlyBudgetHistories_UserId_CategoryId_Year_Month] ON [MonthlyBudgetHistories] ([UserId], [CategoryId], [Year], [Month]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260630174459_AddHistoricalBudgetTracking', N'8.0.0');
GO

COMMIT;
GO

