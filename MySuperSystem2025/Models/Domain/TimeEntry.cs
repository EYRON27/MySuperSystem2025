using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Time entry entity representing tracked time for a specific activity.
    /// Supports both manual entry and timer-based tracking.
    /// </summary>
    public class TimeEntry : BaseEntity
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        /// <summary>
        /// Duration in minutes (calculated automatically)
        /// </summary>
        [Required]
        public int DurationMinutes { get; set; }

        // Foreign keys
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual TimeCategory? Category { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
