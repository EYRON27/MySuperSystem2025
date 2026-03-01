-- Fix one-time budget categories: Restore BudgetAmount to include funds that were
-- previously added as negative expenses WITHOUT increasing BudgetAmount.
-- 
-- The old bug: AddFunds created a negative expense but did NOT increase BudgetAmount.
-- The new fix: AddFunds increases BudgetAmount directly (no more negative expenses).
-- 
-- This script adds the total funds-added amounts back to BudgetAmount for categories
-- that have existing negative "FUNDS ADDED" expense entries.

-- Step 1: Review current state
SELECT 
    ec.Id AS CategoryId,
    ec.Name AS CategoryName,
    ec.BudgetAmount AS CurrentBudget,
    ec.MonthlyFixedBudget,
    COALESCE(SUM(CASE WHEN e.Amount < 0 AND e.Reason LIKE '%[[]FUNDS ADDED]%' AND e.IsDeleted = 0 THEN -e.Amount ELSE 0 END), 0) AS TotalFundsAdded,
    COALESCE(SUM(CASE WHEN e.Amount > 0 AND e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS TotalSpent,
    ec.BudgetAmount + COALESCE(SUM(CASE WHEN e.Amount < 0 AND e.Reason LIKE '%[[]FUNDS ADDED]%' AND e.IsDeleted = 0 THEN -e.Amount ELSE 0 END), 0) AS NewBudget,
    ec.BudgetAmount + COALESCE(SUM(CASE WHEN e.Amount < 0 AND e.Reason LIKE '%[[]FUNDS ADDED]%' AND e.IsDeleted = 0 THEN -e.Amount ELSE 0 END), 0) 
        - COALESCE(SUM(CASE WHEN e.Amount > 0 AND e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS NewRemaining
FROM ExpenseCategories ec
LEFT JOIN Expenses e ON e.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0
GROUP BY ec.Id, ec.Name, ec.BudgetAmount, ec.MonthlyFixedBudget;

-- Step 2: Fix BudgetAmount by adding funds-added totals
UPDATE ec
SET 
    ec.BudgetAmount = ec.BudgetAmount + COALESCE(funds.TotalFundsAdded, 0),
    ec.RemainingAmount = ec.BudgetAmount + COALESCE(funds.TotalFundsAdded, 0) - COALESCE(spent.TotalSpent, 0)
FROM ExpenseCategories ec
LEFT JOIN (
    SELECT 
        CategoryId,
        SUM(CASE WHEN Amount < 0 AND Reason LIKE '%[[]FUNDS ADDED]%' AND IsDeleted = 0 THEN -Amount ELSE 0 END) AS TotalFundsAdded
    FROM Expenses
    GROUP BY CategoryId
) funds ON funds.CategoryId = ec.Id
LEFT JOIN (
    SELECT 
        CategoryId,
        SUM(CASE WHEN Amount > 0 AND IsDeleted = 0 THEN Amount ELSE 0 END) AS TotalSpent
    FROM Expenses
    GROUP BY CategoryId
) spent ON spent.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0
    AND COALESCE(funds.TotalFundsAdded, 0) > 0;

-- Step 3: Clamp RemainingAmount to 0 (no negatives)
UPDATE ExpenseCategories
SET RemainingAmount = 0
WHERE RemainingAmount < 0 AND IsDeleted = 0;

-- Step 4: Verify the fix
SELECT 
    ec.Id AS CategoryId,
    ec.Name AS CategoryName,
    ec.BudgetAmount AS FixedBudget,
    COALESCE(SUM(CASE WHEN e.Amount > 0 AND e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS TotalSpent,
    ec.BudgetAmount - COALESCE(SUM(CASE WHEN e.Amount > 0 AND e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS CalculatedRemaining,
    ec.RemainingAmount AS StoredRemaining
FROM ExpenseCategories ec
LEFT JOIN Expenses e ON e.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0
GROUP BY ec.Id, ec.Name, ec.BudgetAmount, ec.RemainingAmount;

PRINT 'One-time budget fix applied successfully!';
