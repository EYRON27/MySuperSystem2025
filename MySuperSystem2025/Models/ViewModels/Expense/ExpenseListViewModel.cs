namespace MySuperSystem2025.Models.ViewModels.Expense
{
    /// <summary>
    /// View model for expense list items
    /// </summary>
    public class ExpenseListItemViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    /// <summary>
    /// View model for expense list with filtering
    /// </summary>
    public class ExpenseListViewModel
    {
        public List<ExpenseListItemViewModel> Expenses { get; set; } = new();
        public string? FilterPeriod { get; set; }
        public int? FilterCategoryId { get; set; }
        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }

        public List<ExpenseCategoryViewModel> Categories { get; set; } = new();
    }
}
