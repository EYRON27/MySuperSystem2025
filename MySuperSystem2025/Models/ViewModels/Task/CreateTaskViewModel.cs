using System.ComponentModel.DataAnnotations;
using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Models.ViewModels.Task
{
    /// <summary>
    /// View model for creating a new task
    /// </summary>
    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Deadline")]
        [DataType(DataType.DateTime)]
        public DateTime? Deadline { get; set; }
    }

    /// <summary>
    /// View model for editing an existing task
    /// </summary>
    public class EditTaskViewModel : CreateTaskViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Status")]
        public Domain.TaskStatus Status { get; set; }

        public bool IsCompleted => Status == Domain.TaskStatus.Completed;
    }

    /// <summary>
    /// View model for updating task status
    /// </summary>
    public class UpdateTaskStatusViewModel
    {
        public int Id { get; set; }
        public Domain.TaskStatus Status { get; set; }
    }
}
