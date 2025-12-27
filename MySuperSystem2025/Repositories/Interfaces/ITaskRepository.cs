using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Task repository interface with specialized task queries
    /// </summary>
    public interface ITaskRepository : IRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetUserTasksAsync(string userId);
        Task<IEnumerable<TaskItem>> GetUserTasksByStatusAsync(string userId, Models.Domain.TaskStatus status);
        Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(string userId);
        Task<TaskItem?> GetTaskByIdAndUserAsync(int id, string userId);
        Task<int> GetPendingCountAsync(string userId);
        Task<int> GetCompletedCountAsync(string userId);
        Task<int> GetOverdueCountAsync(string userId);
    }
}
