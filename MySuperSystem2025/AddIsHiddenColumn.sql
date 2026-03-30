-- Add IsHidden column to ExpenseCategories table
-- This allows users to hide one-time budget categories from the dashboard and dropdowns

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'ExpenseCategories') AND name = 'IsHidden'
)
BEGIN
    ALTER TABLE ExpenseCategories ADD IsHidden BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsHidden column to ExpenseCategories table.';
END
ELSE
BEGIN
    PRINT 'IsHidden column already exists.';
END
