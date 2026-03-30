namespace MySuperSystem2025.Models.ViewModels.Expense
{
    /// <summary>
    /// Dashboard view model containing expense summaries and balance tracking
    /// </summary>
    public class ExpenseDashboardViewModel
    {
        public decimal TodayTotal { get; set; }
        public decimal WeeklyTotal { get; set; }
        public decimal MonthlyTotal { get; set; }
        public decimal YearlyTotal { get; set; }

        public int TodayCount { get; set; }
        public int WeeklyCount { get; set; }
        public int MonthlyCount { get; set; }
        public int YearlyCount { get; set; }

        /// <summary>
        /// Total budget across all categories
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// Total remaining balance across all categories
        /// </summary>
        public decimal TotalRemainingBalance { get; set; }

        /// <summary>
        /// Total expenses (all time)
        /// </summary>
        public decimal TotalExpenses { get; set; }

        public List<ExpenseListItemViewModel> RecentExpenses { get; set; } = new();
        public List<CategorySummaryViewModel> CategoryBreakdown { get; set; } = new();

        /// <summary>
        /// Category balance cards for dashboard display
        /// </summary>
        public List<CategoryBalanceViewModel> CategoryBalances { get; set; } = new();

        /// <summary>
        /// Current period filter for category breakdown
        /// </summary>
        public string BreakdownPeriod { get; set; } = "monthly";

        /// <summary>
        /// Display name for the breakdown period
        /// </summary>
        public string BreakdownPeriodName { get; set; } = "This Month";

        /// <summary>
        /// Selected month for filtering (format: "yyyy-MM")
        /// </summary>
        public string? SelectedMonth { get; set; }

        /// <summary>
        /// Available months for dropdown (from December 2025 to current)
        /// </summary>
        public List<MonthOption> AvailableMonths { get; set; } = new();

        /// <summary>
        /// Total monthly fixed budget (sum of all categories)
        /// </summary>
        public decimal TotalMonthlyFixedBudget { get; set; }

        /// <summary>
        /// Total spent in selected month (ALL categories - for reporting)
        /// </summary>
        public decimal SelectedMonthTotal { get; set; }

        /// <summary>
        /// Spent this month from MONTHLY BUDGET categories only (for budget deduction)
        /// </summary>
        public decimal MonthlyBudgetSpentThisMonth { get; set; }

        /// <summary>
        /// Remaining balance for selected month (monthly budgets only)
        /// </summary>
        public decimal SelectedMonthRemaining { get; set; }

        /// <summary>
        /// Total accumulated savings from past months (monthly budgets only).
        /// Savings = SUM of (MonthlyFixedBudget - SpentThatMonth) for each completed past month.
        /// Negative months (overspent) are clamped to 0 so they don't reduce savings.
        /// </summary>
        public decimal TotalMonthlySavings { get; set; }
    }

    /// <summary>
    /// View model for the monthly savings breakdown page
    /// </summary>
    public class MonthlySavingsViewModel
    {
        public decimal GrandTotalSavings { get; set; }
        public decimal TotalMonthlyFixedBudget { get; set; }
        public List<CategorySavingsViewModel> Categories { get; set; } = new();
        public List<MonthSavingsRow> MonthlyBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Per-category savings totals
    /// </summary>
    public class CategorySavingsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal MonthlyFixedBudget { get; set; }
        public decimal TotalSaved { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalBudgeted { get; set; }
        public int MonthCount { get; set; }
    }

    /// <summary>
    /// One row per past month with per-category breakdown
    /// </summary>
    public class MonthSavingsRow
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthDisplay { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalSaved { get; set; }
        public List<CategoryMonthSaving> CategorySavings { get; set; } = new();
    }

    /// <summary>
    /// Savings for a single category in a single month
    /// </summary>
    public class CategoryMonthSaving
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Saved { get; set; }
    }

    /// <summary>
    /// Month option for dropdown
    /// </summary>
    public class MonthOption
    {
        public string Value { get; set; } = string.Empty; // "yyyy-MM"
        public string Display { get; set; } = string.Empty; // "December 2025"
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Category summary for dashboard charts
    /// </summary>
    public class CategorySummaryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Category balance for dashboard display cards
    /// </summary>
    public class CategoryBalanceViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal MonthlyFixedBudget { get; set; }
        
        /// <summary>
        /// Whether the monthly budget is currently active (not paused)
        /// </summary>
        public bool IsBudgetActive { get; set; } = true;

        /// <summary>
        /// Amount spent in the selected/current month (for display purposes)
        /// </summary>
        public decimal SpentThisMonth { get; set; }
        
        /// <summary>
        /// Total expenses against this category (all-time for one-time budgets)
        /// </summary>
        public decimal TotalExpenses { get; set; }
        
        public decimal UsagePercentage => BudgetAmount > 0 ? Math.Round((TotalExpenses / BudgetAmount) * 100, 1) : 0;
        public bool IsLowBalance => HasMonthlyBudget && !IsPaused && BudgetAmount > 0 && RemainingAmount <= (BudgetAmount * 0.2m);
        public bool HasBudget => BudgetAmount > 0;
        public bool HasMonthlyBudget => MonthlyFixedBudget > 0;
        public bool IsPaused => MonthlyFixedBudget > 0 && !IsBudgetActive;

        /// <summary>
        /// Whether this category is hidden from dashboard and dropdowns
        /// </summary>
        public bool IsHidden { get; set; }
    }
}

