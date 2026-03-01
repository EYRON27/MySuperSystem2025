-- =============================================================================
-- DEFINITIVE FIX for one-time budget categories (double-counted funds)
-- =============================================================================
-- 
-- The bug: BudgetAmount got inflated because funds were counted twice:
--   1. New code increased BudgetAmount when adding funds
--   2. Old SQL fix script ALSO added funds to BudgetAmount
--
-- Detection: If (BudgetAmount - TotalFundsAdded) < 0, BudgetAmount is inflated.
--   TotalFundsAdded = SUM of ABS(negative [FUNDS ADDED] expenses)
--
-- Fix: Set BudgetAmount = TotalFundsAdded (assumes original budget was ~0).
--   Then: RemainingAmount = BudgetAmount - TotalSpent.
--
-- For categories WITHOUT double-counting: just sync RemainingAmount.
-- =============================================================================

-- Step 1: DIAGNOSTIC - Show current state and detect double-counting
SELECT 
    ec.Id,
    ec.Name,
    ec.BudgetAmount AS StoredBudget,
    ec.RemainingAmount AS StoredRemaining,
    COALESCE(pos.TotalSpent, 0) AS TotalSpent,
    COALESCE(neg.TotalFundsAdded, 0) AS TotalFundsAdded,
    ec.BudgetAmount - COALESCE(neg.TotalFundsAdded, 0) AS ImpliedOriginalBudget,
    CASE 
        WHEN ec.BudgetAmount - COALESCE(neg.TotalFundsAdded, 0) < 0 
        THEN 'INFLATED - will fix'
        ELSE 'OK'
    END AS Status,
    -- What BudgetAmount SHOULD be after fix:
    CASE 
        WHEN ec.BudgetAmount - COALESCE(neg.TotalFundsAdded, 0) < 0 
        THEN COALESCE(neg.TotalFundsAdded, 0)
        ELSE ec.BudgetAmount
    END AS CorrectedBudget,
    -- What Remaining SHOULD be after fix:
    CASE 
        WHEN ec.BudgetAmount - COALESCE(neg.TotalFundsAdded, 0) < 0 
        THEN CASE WHEN COALESCE(neg.TotalFundsAdded, 0) - COALESCE(pos.TotalSpent, 0) > 0
             THEN COALESCE(neg.TotalFundsAdded, 0) - COALESCE(pos.TotalSpent, 0)
             ELSE 0 END
        ELSE CASE WHEN ec.BudgetAmount - COALESCE(pos.TotalSpent, 0) > 0
             THEN ec.BudgetAmount - COALESCE(pos.TotalSpent, 0)
             ELSE 0 END
    END AS CorrectedRemaining
FROM ExpenseCategories ec
LEFT JOIN (
    SELECT CategoryId, SUM(Amount) AS TotalSpent
    FROM Expenses WHERE IsDeleted = 0 AND Amount > 0
    GROUP BY CategoryId
) pos ON pos.CategoryId = ec.Id
LEFT JOIN (
    SELECT CategoryId, SUM(-Amount) AS TotalFundsAdded
    FROM Expenses WHERE IsDeleted = 0 AND Amount < 0
    GROUP BY CategoryId
) neg ON neg.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 AND ec.MonthlyFixedBudget = 0 AND ec.BudgetAmount > 0
ORDER BY ec.Name;

-- Step 2: FIX INFLATED categories (where implied original < 0)
-- Set BudgetAmount = TotalFundsAdded (removes double-count)
UPDATE ec
SET 
    ec.BudgetAmount = COALESCE(neg.TotalFundsAdded, 0),
    ec.RemainingAmount = CASE 
        WHEN COALESCE(neg.TotalFundsAdded, 0) - COALESCE(pos.TotalSpent, 0) > 0
        THEN COALESCE(neg.TotalFundsAdded, 0) - COALESCE(pos.TotalSpent, 0)
        ELSE 0 
    END
FROM ExpenseCategories ec
LEFT JOIN (
    SELECT CategoryId, SUM(Amount) AS TotalSpent
    FROM Expenses WHERE IsDeleted = 0 AND Amount > 0
    GROUP BY CategoryId
) pos ON pos.CategoryId = ec.Id
LEFT JOIN (
    SELECT CategoryId, SUM(-Amount) AS TotalFundsAdded
    FROM Expenses WHERE IsDeleted = 0 AND Amount < 0
    GROUP BY CategoryId
) neg ON neg.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 
    AND ec.MonthlyFixedBudget = 0 
    AND ec.BudgetAmount > 0
    AND ec.BudgetAmount - COALESCE(neg.TotalFundsAdded, 0) < 0;

-- Step 3: SYNC RemainingAmount for non-inflated categories  
UPDATE ec
SET ec.RemainingAmount = CASE 
        WHEN ec.BudgetAmount - COALESCE(pos.TotalSpent, 0) > 0
        THEN ec.BudgetAmount - COALESCE(pos.TotalSpent, 0)
        ELSE 0 
    END
FROM ExpenseCategories ec
LEFT JOIN (
    SELECT CategoryId, SUM(Amount) AS TotalSpent
    FROM Expenses WHERE IsDeleted = 0 AND Amount > 0
    GROUP BY CategoryId
) pos ON pos.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 AND ec.MonthlyFixedBudget = 0 AND ec.BudgetAmount > 0;

-- Step 4: Also fix monthly budget categories (sync RemainingAmount for current month)
DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
DECLARE @MonthEnd DATE = DATEADD(MONTH, 1, @MonthStart);

UPDATE ec
SET ec.RemainingAmount = CASE
        WHEN ec.BudgetAmount - COALESCE(pos.TotalSpent, 0) > 0
        THEN ec.BudgetAmount - COALESCE(pos.TotalSpent, 0)
        ELSE 0
    END
FROM ExpenseCategories ec
LEFT JOIN (
    SELECT CategoryId, SUM(Amount) AS TotalSpent
    FROM Expenses
    WHERE IsDeleted = 0 AND Amount > 0 AND Date >= @MonthStart AND Date < @MonthEnd
    GROUP BY CategoryId
) pos ON pos.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 AND ec.MonthlyFixedBudget > 0;

-- Step 5: VERIFY
SELECT 
    ec.Id,
    ec.Name,
    CASE WHEN ec.MonthlyFixedBudget > 0 THEN 'Monthly' ELSE 'One-Time' END AS BudgetType,
    ec.BudgetAmount,
    ec.RemainingAmount,
    COALESCE(pos.TotalSpent, 0) AS TotalSpent,
    COALESCE(neg.TotalFundsAdded, 0) AS TotalFundsAdded
FROM ExpenseCategories ec
LEFT JOIN (
    SELECT CategoryId, SUM(Amount) AS TotalSpent
    FROM Expenses WHERE IsDeleted = 0 AND Amount > 0
    GROUP BY CategoryId
) pos ON pos.CategoryId = ec.Id
LEFT JOIN (
    SELECT CategoryId, SUM(-Amount) AS TotalFundsAdded
    FROM Expenses WHERE IsDeleted = 0 AND Amount < 0
    GROUP BY CategoryId
) neg ON neg.CategoryId = ec.Id
WHERE ec.IsDeleted = 0 AND ec.BudgetAmount > 0
ORDER BY ec.Name;

PRINT 'All budget categories fixed successfully!';
