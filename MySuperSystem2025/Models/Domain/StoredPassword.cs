using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Stored password entity for secure credential storage.
    /// Passwords are encrypted using AES encryption - never stored as plain text.
    /// </summary>
    public class StoredPassword : BaseEntity
    {
        [Required(ErrorMessage = "Website/App name is required")]
        [StringLength(100, ErrorMessage = "Website/App name cannot exceed 100 characters")]
        public string WebsiteOrAppName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(255)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Encrypted password - stored using AES encryption
        /// Never store plain text passwords
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string EncryptedPassword { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(255)]
        public string? WebsiteUrl { get; set; }

        // Foreign keys
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual PasswordCategory? Category { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
