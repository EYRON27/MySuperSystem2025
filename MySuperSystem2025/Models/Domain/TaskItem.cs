using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Task status enumeration for tracking task progress
    /// </summary>
    public enum TaskStatus
    {
        ToDo = 0,
        Ongoing = 1,
        Completed = 2
    }

    /// <summary>
    /// Task/To-Do item entity for task management.
    /// Supports status tracking and deadline management.
    /// </summary>
    public class TaskItem : BaseEntity
    {
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        public string? Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        [DataType(DataType.DateTime)]
        public DateTime? Deadline { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// Determines if the task is overdue based on deadline
        /// </summary>
        [NotMapped]
        public bool IsOverdue => Deadline.HasValue && 
                                 Deadline.Value < DateTime.Now && 
                                 Status != TaskStatus.Completed;
    }
}
