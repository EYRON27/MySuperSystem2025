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
        public decimal TotalExpenses => BudgetAmount - RemainingAmount;
        public decimal UsagePercentage => BudgetAmount > 0 ? Math.Round((TotalExpenses / BudgetAmount) * 100, 1) : 0;
        public bool IsLowBalance => BudgetAmount > 0 && RemainingAmount <= (BudgetAmount * 0.2m);
        public bool HasBudget => BudgetAmount > 0;
    }
}
