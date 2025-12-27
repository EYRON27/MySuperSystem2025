using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Task repository implementation with specialized queries
    /// </summary>
    public class TaskRepository : Repository<TaskItem>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskItem>> GetUserTasksAsync(string userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.Deadline)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetUserTasksByStatusAsync(string userId, Models.Domain.TaskStatus status)
        {
            return await _dbSet
                .Where(t => t.UserId == userId && t.Status == status)
                .OrderBy(t => t.Deadline)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(string userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(t => t.UserId == userId && 
                           t.Deadline.HasValue && 
                           t.Deadline.Value < now && 
                           t.Status != Models.Domain.TaskStatus.Completed)
                .OrderBy(t => t.Deadline)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAndUserAsync(int id, string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<int> GetPendingCountAsync(string userId)
        {
            return await _dbSet
                .CountAsync(t => t.UserId == userId && t.Status == Models.Domain.TaskStatus.ToDo);
        }

        public async Task<int> GetCompletedCountAsync(string userId)
        {
            return await _dbSet
                .CountAsync(t => t.UserId == userId && t.Status == Models.Domain.TaskStatus.Completed);
        }

        public async Task<int> GetOverdueCountAsync(string userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .CountAsync(t => t.UserId == userId && 
                                t.Deadline.HasValue && 
                                t.Deadline.Value < now && 
                                t.Status != Models.Domain.TaskStatus.Completed);
        }
    }
}
