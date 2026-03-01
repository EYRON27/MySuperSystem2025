-- Fix BudgetAmount for one-time budget categories that were incorrectly deflated
-- by the FixInflatedOneTimeBudgetsAsync method.
--
-- The old bug flow was:
-- 1. AddFunds increased BudgetAmount AND recorded negative expense (double-counting)
-- 2. FixInflatedOneTimeBudgetsAsync subtracted funds-added from BudgetAmount (undid step 1)
-- 3. But AddFundsToCategoryAsync was also fixed to NOT increase BudgetAmount
-- 4. Result: BudgetAmount is now too LOW because the fix subtracted what was never added again
--
-- The correct BudgetAmount should be restored by ADDING BACK the funds-added totals
-- since the negative expenses in the Expenses table already handle the balance calculation.

-- Step 1: Review current state (run this first to see what needs fixing)
SELECT 
    ec.Id AS CategoryId,
    ec.Name AS CategoryName,
    ec.BudgetAmount AS CurrentBudget,
    ec.MonthlyFixedBudget,
    COALESCE(SUM(CASE WHEN e.Amount < 0 AND e.Reason LIKE '[[]FUNDS ADDED]%' AND e.IsDeleted = 0 THEN -e.Amount ELSE 0 END), 0) AS TotalFundsAdded,
    COALESCE(SUM(CASE WHEN e.Amount > 0 AND e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS TotalSpent,
    COALESCE(SUM(CASE WHEN e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS NetExpenses
FROM ExpenseCategories ec
LEFT JOIN Expenses e ON e.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0
GROUP BY ec.Id, ec.Name, ec.BudgetAmount, ec.MonthlyFixedBudget;

-- Step 2: Fix the BudgetAmount by adding back the funds-added amounts
-- (Only run this if Step 1 shows categories that were deflated)
UPDATE ec
SET ec.BudgetAmount = ec.BudgetAmount + COALESCE(funds.TotalFundsAdded, 0)
FROM ExpenseCategories ec
INNER JOIN (
    SELECT 
        CategoryId,
        SUM(CASE WHEN Amount < 0 AND Reason LIKE '[[]FUNDS ADDED]%' AND IsDeleted = 0 THEN -Amount ELSE 0 END) AS TotalFundsAdded
    FROM Expenses
    GROUP BY CategoryId
    HAVING SUM(CASE WHEN Amount < 0 AND Reason LIKE '[[]FUNDS ADDED]%' AND IsDeleted = 0 THEN -Amount ELSE 0 END) > 0
) funds ON funds.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0;

-- Step 3: Verify the fix
SELECT 
    ec.Id AS CategoryId,
    ec.Name AS CategoryName,
    ec.BudgetAmount AS FixedBudget,
    COALESCE(SUM(CASE WHEN e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS NetExpenses,
    ec.BudgetAmount - COALESCE(SUM(CASE WHEN e.IsDeleted = 0 THEN e.Amount ELSE 0 END), 0) AS CalculatedRemaining
FROM ExpenseCategories ec
LEFT JOIN Expenses e ON e.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0
GROUP BY ec.Id, ec.Name, ec.BudgetAmount;
