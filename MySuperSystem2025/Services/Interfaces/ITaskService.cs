using MySuperSystem2025.Models.ViewModels.Task;
using TaskStatus = MySuperSystem2025.Models.Domain.TaskStatus;

namespace MySuperSystem2025.Services.Interfaces
{
    /// <summary>
    /// Task service interface for business logic operations
    /// </summary>
    public interface ITaskService
    {
        Task<TaskDashboardViewModel> GetDashboardAsync(string userId);
        Task<List<TaskListItemViewModel>> GetTasksAsync(string userId, TaskStatus? status = null);
        Task<EditTaskViewModel?> GetTaskForEditAsync(int id, string userId);
        Task<bool> CreateTaskAsync(CreateTaskViewModel model, string userId);
        Task<bool> UpdateTaskAsync(EditTaskViewModel model, string userId);
        Task<bool> UpdateTaskStatusAsync(int id, TaskStatus status, string userId);
        Task<bool> DeleteTaskAsync(int id, string userId);
    }
}
