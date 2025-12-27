using System.ComponentModel.DataAnnotations;

namespace MySuperSystem2025.Models.ViewModels.Expense
{
    /// <summary>
    /// View model for expense category display
    /// </summary>
    public class ExpenseCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public int ExpenseCount { get; set; }
    }

    /// <summary>
    /// View model for creating/editing expense categories
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
    }

    /// <summary>
    /// View model for editing expense categories
    /// </summary>
    public class EditExpenseCategoryViewModel : CreateExpenseCategoryViewModel
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
    }
}
