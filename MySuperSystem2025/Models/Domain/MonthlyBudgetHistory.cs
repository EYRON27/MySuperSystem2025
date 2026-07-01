using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Stores the historical budget configuration for a category for a specific month.
    /// This prevents past dashboards from being altered when current budget settings change.
    /// </summary>
    public class MonthlyBudgetHistory : BaseEntity
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        /// <summary>
        /// The fixed monthly budget base amount active in this specific month
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseBudget { get; set; }

        /// <summary>
        /// Any rollover amount carried into this month
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RolloverAmount { get; set; }

        /// <summary>
        /// True if the budget was considered active during this month (not paused)
        /// </summary>
        public bool IsBudgetActive { get; set; } = true;

        [ForeignKey("CategoryId")]
        public virtual ExpenseCategory? Category { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
