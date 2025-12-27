using System.ComponentModel.DataAnnotations;

namespace MySuperSystem2025.Models.ViewModels.Password
{
    /// <summary>
    /// View model for password category display
    /// </summary>
    public class PasswordCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public int PasswordCount { get; set; }
    }

    /// <summary>
    /// View model for creating password categories
    /// </summary>
    public class CreatePasswordCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// View model for editing password categories
    /// </summary>
    public class EditPasswordCategoryViewModel : CreatePasswordCategoryViewModel
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
    }
}
