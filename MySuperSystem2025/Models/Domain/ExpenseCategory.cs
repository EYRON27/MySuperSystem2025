using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Expense category entity for organizing expenses.
    /// Users can create custom categories in addition to default ones.
    /// Includes budget tracking with BudgetAmount and RemainingAmount.
    /// </summary>
    public class ExpenseCategory : BaseEntity
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Category name can only contain letters, numbers, and spaces")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if this is a system default category
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Total budget/money allocated to this category
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Budget amount must be a positive value")]
        public decimal BudgetAmount { get; set; } = 0;

        /// <summary>
        /// Remaining balance after expenses are deducted
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; } = 0;

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        /// <summary>
        /// Calculated total expenses for this category
        /// </summary>
        [NotMapped]
        public decimal TotalExpenses => BudgetAmount - RemainingAmount;

        /// <summary>
        /// Percentage of budget used
        /// </summary>
        [NotMapped]
        public decimal UsagePercentage => BudgetAmount > 0 ? Math.Round((TotalExpenses / BudgetAmount) * 100, 1) : 0;

        /// <summary>
        /// Indicates if remaining balance is low (below 20%)
        /// </summary>
        [NotMapped]
        public bool IsLowBalance => BudgetAmount > 0 && RemainingAmount <= (BudgetAmount * 0.2m);
    }
}
