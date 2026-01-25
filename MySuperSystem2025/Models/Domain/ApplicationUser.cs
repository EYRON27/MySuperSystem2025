using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Application user extending ASP.NET Identity.
    /// Contains navigation properties to all user-owned entities.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<ExpenseCategory> ExpenseCategories { get; set; } = new List<ExpenseCategory>();
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<StoredPassword> StoredPasswords { get; set; } = new List<StoredPassword>();
        public virtual ICollection<PasswordCategory> PasswordCategories { get; set; } = new List<PasswordCategory>();
        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public virtual ICollection<TimeCategory> TimeCategories { get; set; } = new List<TimeCategory>();
        public virtual ICollection<FoodEntry> FoodEntries { get; set; } = new List<FoodEntry>();
    }
}
