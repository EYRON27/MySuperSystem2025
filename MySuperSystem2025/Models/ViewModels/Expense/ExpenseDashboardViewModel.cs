namespace MySuperSystem2025.Models.ViewModels.Expense
{
    /// <summary>
    /// Dashboard view model containing expense summaries
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

        public List<ExpenseListItemViewModel> RecentExpenses { get; set; } = new();
        public List<CategorySummaryViewModel> CategoryBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Category summary for dashboard charts
    /// </summary>
    public class CategorySummaryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
