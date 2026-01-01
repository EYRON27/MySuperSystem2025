using System.ComponentModel.DataAnnotations;

namespace MySuperSystem2025.Models.ViewModels.Expense
{
    /// <summary>
    /// View model for expense category display with balance tracking
    /// </summary>
    public class ExpenseCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public int ExpenseCount { get; set; }

        /// <summary>
        /// Total budget allocated to this category
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Remaining balance after expenses
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Total expenses (Budget - Remaining)
        /// </summary>
        public decimal TotalExpenses => BudgetAmount - RemainingAmount;

        /// <summary>
        /// Percentage of budget used
        /// </summary>
        public decimal UsagePercentage => BudgetAmount > 0 ? Math.Round((TotalExpenses / BudgetAmount) * 100, 1) : 0;

        /// <summary>
        /// Indicates if remaining balance is low (below 20%)
        /// </summary>
        public bool IsLowBalance => BudgetAmount > 0 && RemainingAmount <= (BudgetAmount * 0.2m);
    }

    /// <summary>
    /// View model for creating/editing expense categories with budget
    /// </summary>
    public class CreateExpenseCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Category name can only contain letters, numbers, and spaces")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget amount must be a positive value")]
        [Display(Name = "Budget Amount (?)")]
        public decimal BudgetAmount { get; set; } = 0;
    }

    /// <summary>
    /// View model for editing expense categories
    /// </summary>
    public class EditExpenseCategoryViewModel : CreateExpenseCategoryViewModel
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }

        /// <summary>
        /// Current remaining balance (read-only display)
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Total expenses already recorded
        /// </summary>
        public decimal TotalExpenses { get; set; }
    }
}
