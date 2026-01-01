-- Script to recalculate RemainingAmount for all categories based on actual expenses
-- Run this against your MySuperSystem2025 database to fix incorrect balances

-- Recalculate RemainingAmount for all categories
UPDATE ec
SET ec.RemainingAmount = ec.BudgetAmount - ISNULL(
    (SELECT SUM(e.Amount) 
     FROM Expenses e 
     WHERE e.CategoryId = ec.Id 
       AND e.IsDeleted = 0
       AND e.UserId = ec.UserId), 
    0)
FROM ExpenseCategories ec
WHERE ec.IsDeleted = 0;

-- Show results
SELECT 
    ec.Name as CategoryName,
    ec.BudgetAmount,
    ISNULL((SELECT SUM(e.Amount) 
            FROM Expenses e 
            WHERE e.CategoryId = ec.Id 
              AND e.IsDeleted = 0
              AND e.UserId = ec.UserId), 0) as TotalExpenses,
    ec.RemainingAmount,
    ec.BudgetAmount - ec.RemainingAmount as CalculatedExpenses
FROM ExpenseCategories ec
WHERE ec.IsDeleted = 0
ORDER BY ec.Name;

PRINT 'Category balances recalculated successfully!';
