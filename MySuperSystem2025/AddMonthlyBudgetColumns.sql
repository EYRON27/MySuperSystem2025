-- Migration Script: Add Monthly Fixed Budget columns to ExpenseCategories
-- Run this script against your MySuperSystem2025 database

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ExpenseCategories') AND name = 'MonthlyFixedBudget')
BEGIN
    ALTER TABLE ExpenseCategories ADD MonthlyFixedBudget DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added MonthlyFixedBudget column';
END
ELSE
BEGIN
    PRINT 'MonthlyFixedBudget column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ExpenseCategories') AND name = 'LastResetYear')
BEGIN
    ALTER TABLE ExpenseCategories ADD LastResetYear INT NULL;
    PRINT 'Added LastResetYear column';
END
ELSE
BEGIN
    PRINT 'LastResetYear column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ExpenseCategories') AND name = 'LastResetMonth')
BEGIN
    ALTER TABLE ExpenseCategories ADD LastResetMonth INT NULL;
    PRINT 'Added LastResetMonth column';
END
ELSE
BEGIN
    PRINT 'LastResetMonth column already exists';
END

-- Set default monthly budgets as per requirements (OPTIONAL - run only if you want preset values)
-- Comment out this section if you want to set them manually
/*
-- Monthly Savings with my GF: ?400
UPDATE ExpenseCategories SET MonthlyFixedBudget = 400 WHERE Name = 'Monthly Savings with my GF';

-- For Our Dates: ?600
UPDATE ExpenseCategories SET MonthlyFixedBudget = 600 WHERE Name = 'For Our Dates';

-- Hygiene Expenses: ?600
UPDATE ExpenseCategories SET MonthlyFixedBudget = 600 WHERE Name = 'Hygiene Expenses';

-- School Expenses: ?3000
UPDATE ExpenseCategories SET MonthlyFixedBudget = 3000 WHERE Name = 'School Expenses';

-- Game Expenses: ?200
UPDATE ExpenseCategories SET MonthlyFixedBudget = 200 WHERE Name = 'Game Expenses';

-- Bills Expenses: ?200
UPDATE ExpenseCategories SET MonthlyFixedBudget = 200 WHERE Name = 'Bills Expenses';
*/

PRINT 'Migration completed successfully!';

-- Verify the changes
SELECT 
    Id,
    Name,
    BudgetAmount,
    RemainingAmount,
    MonthlyFixedBudget,
    LastResetYear,
    LastResetMonth
FROM ExpenseCategories
WHERE IsDeleted = 0
ORDER BY Name;
