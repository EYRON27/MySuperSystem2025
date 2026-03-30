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
        /// Fixed monthly budget that resets every month
        /// </summary>
        public decimal MonthlyFixedBudget { get; set; }

        /// <summary>
        /// Whether the monthly budget is currently active (not paused)
        /// </summary>
        public bool IsBudgetActive { get; set; } = true;

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

        /// <summary>
        /// Indicates if this category has monthly fixed budget
        /// </summary>
        public bool HasMonthlyBudget => MonthlyFixedBudget > 0;

        /// <summary>
        /// Whether this monthly budget is currently paused
        /// </summary>
        public bool IsPaused => MonthlyFixedBudget > 0 && !IsBudgetActive;

        /// <summary>
        /// Whether this category is hidden from dashboard and dropdowns
        /// </summary>
        public bool IsHidden { get; set; }
    }

    /// <summary>
    /// View model for creating/editing expense categories with budget
    /// /// </summary>
    public class CreateExpenseCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\.\,\'\(\)\&]+$", ErrorMessage = "Category name can only contain letters, numbers, spaces, and common punctuation (- . , ' ( ) &)")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget amount must be a positive value")]
        [Display(Name = "Initial Budget Amount (?)")]
        public decimal BudgetAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Monthly budget must be a positive value")]
        [Display(Name = "Monthly Fixed Budget (?) - Resets every month")]
        public decimal MonthlyFixedBudget { get; set; } = 0;

        /// <summary>
        /// Whether the monthly budget is active. Set to false to pause resets.
        /// </summary>
        [Display(Name = "Budget Active")]
        public bool IsBudgetActive { get; set; } = true;

        /// <summary>
        /// Whether unspent budget rolls over to the next month.
        /// </summary>
        [Display(Name = "Rollover Savings")]
        public bool RolloverEnabled { get; set; } = false;
    }

    /// <summary>
    /// View model for editing expense categories
    /// /// </summary>
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

    /// <summary>
    /// View model for adding funds back to a one-time budget category
    /// </summary>
    public class AddFundsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal CurrentBudget { get; set; }
        public decimal CurrentRemaining { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount to Add (?)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters")]
        [Display(Name = "Reason / Description")]
        public string Reason { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;
    }

    /// <summary>
    /// View model for editing an existing funds-added entry
    /// /// </summary>
    public class EditFundsViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount (?)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters")]
        [Display(Name = "Reason / Description")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;
    }
}

