-- Migration Script: Add Budget Tracking to ExpenseCategory
-- Run this script against your MySuperSystem2025 database

-- Add BudgetAmount column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseCategories]') AND name = 'BudgetAmount')
BEGIN
    ALTER TABLE [dbo].[ExpenseCategories]
    ADD [BudgetAmount] decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added BudgetAmount column to ExpenseCategories';
END
ELSE
BEGIN
    PRINT 'BudgetAmount column already exists';
END

-- Add RemainingAmount column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseCategories]') AND name = 'RemainingAmount')
BEGIN
    ALTER TABLE [dbo].[ExpenseCategories]
    ADD [RemainingAmount] decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added RemainingAmount column to ExpenseCategories';
END
ELSE
BEGIN
    PRINT 'RemainingAmount column already exists';
END

PRINT 'Budget tracking columns added successfully!';
