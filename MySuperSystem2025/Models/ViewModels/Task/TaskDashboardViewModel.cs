using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Models.ViewModels.Task
{
    /// <summary>
    /// Task dashboard view model with summary statistics
    /// </summary>
    public class TaskDashboardViewModel
    {
        public int PendingCount { get; set; }
        public int OngoingCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
        public int TotalCount { get; set; }

        public List<TaskListItemViewModel> ToDoTasks { get; set; } = new();
        public List<TaskListItemViewModel> OngoingTasks { get; set; } = new();
        public List<TaskListItemViewModel> CompletedTasks { get; set; } = new();
        public List<TaskListItemViewModel> OverdueTasks { get; set; } = new();
    }

    /// <summary>
    /// Task list item view model
    /// </summary>
    public class TaskListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Domain.TaskStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            Domain.TaskStatus.ToDo => "To Do",
            Domain.TaskStatus.Ongoing => "Ongoing",
            Domain.TaskStatus.Completed => "Completed",
            _ => "Unknown"
        };
        public string StatusBadgeClass => Status switch
        {
            Domain.TaskStatus.ToDo => "bg-secondary",
            Domain.TaskStatus.Ongoing => "bg-primary",
            Domain.TaskStatus.Completed => "bg-success",
            _ => "bg-secondary"
        };
        public DateTime? Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
