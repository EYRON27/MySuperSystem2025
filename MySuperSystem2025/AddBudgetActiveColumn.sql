-- Add IsBudgetActive column to ExpenseCategories
-- This allows pausing/resuming monthly budget resets per category
-- Default is 1 (active) so all existing categories continue working normally

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ExpenseCategories' AND COLUMN_NAME = 'IsBudgetActive')
BEGIN
    ALTER TABLE ExpenseCategories ADD IsBudgetActive BIT NOT NULL DEFAULT 1;
    PRINT 'Added IsBudgetActive column to ExpenseCategories';
END
ELSE
BEGIN
    PRINT 'IsBudgetActive column already exists';
END
