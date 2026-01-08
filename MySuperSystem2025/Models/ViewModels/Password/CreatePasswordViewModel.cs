using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MySuperSystem2025.Models.ViewModels.Password
{
    /// <summary>
    /// View model for creating a new stored password
    /// </summary>
    public class CreatePasswordViewModel
    {
        [Required(ErrorMessage = "Website/App name is required")]
        [StringLength(100, ErrorMessage = "Website/App name cannot exceed 100 characters")]
        [Display(Name = "Website / App Name")]
        public string WebsiteOrAppName { get; set; } = string.Empty;

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Website URL")]
        public string? WebsiteUrl { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(255)]
        [Display(Name = "Username / Email")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public SelectList? Categories { get; set; }
    }

    /// <summary>
    /// View model for editing a stored password with secure verification
    /// </summary>
    public class EditPasswordViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Website/App name is required")]
        [StringLength(100, ErrorMessage = "Website/App name cannot exceed 100 characters")]
        [Display(Name = "Website / App Name")]
        public string WebsiteOrAppName { get; set; } = string.Empty;

        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Website URL")]
        public string? WebsiteUrl { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(255)]
        [Display(Name = "Username / Email")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Current stored password (user must enter to verify before changing)
        /// Required when changing the password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Current Stored Password")]
        public string? CurrentPassword { get; set; }

        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        public string? ConfirmNewPassword { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public SelectList? Categories { get; set; }

        /// <summary>
        /// Indicates if the user wants to change the password
        /// </summary>
        public bool IsChangingPassword => !string.IsNullOrWhiteSpace(NewPassword);
    }

    /// <summary>
    /// View model for revealing a password (requires re-authentication)
    /// </summary>
    public class RevealPasswordViewModel
    {
        public int PasswordId { get; set; }

        [Required(ErrorMessage = "Please enter your account password to reveal this password")]
        [DataType(DataType.Password)]
        [Display(Name = "Your Account Password")]
        public string AccountPassword { get; set; } = string.Empty;
    }
}
