using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Password category entity for organizing stored credentials.
    /// Users can create custom categories in addition to defaults.
    /// </summary>
    public class PasswordCategory : BaseEntity
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if this is a system default category
        /// </summary>
        public bool IsDefault { get; set; } = false;

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<StoredPassword> StoredPasswords { get; set; } = new List<StoredPassword>();
    }
}
