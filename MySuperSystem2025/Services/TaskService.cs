using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Task;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services.Interfaces;
using TaskStatus = MySuperSystem2025.Models.Domain.TaskStatus;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Task service implementation handling all task-related business logic
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TaskService> _logger;

        public TaskService(IUnitOfWork unitOfWork, ILogger<TaskService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data with task summaries
        /// </summary>
        public async Task<TaskDashboardViewModel> GetDashboardAsync(string userId)
        {
            var allTasks = await _unitOfWork.Tasks.GetUserTasksAsync(userId);
            var tasksList = allTasks.ToList();

            _logger.LogInformation("GetDashboardAsync: Retrieved {Count} tasks for user {UserId}", tasksList.Count, userId);

            var now = DateTime.Now;
            var overdueTasks = tasksList
                .Where(t => t.Deadline.HasValue && t.Deadline.Value < now && t.Status != TaskStatus.Completed)
                .ToList();

            var overdueTaskIds = overdueTasks.Select(t => t.Id).ToHashSet();

            // Log deadline information for debugging
            foreach (var task in tasksList.Take(5))
            {
                _logger.LogInformation("Task {TaskId} ({Title}): Deadline={Deadline}, Status={Status}", 
                    task.Id, task.Title, task.Deadline?.ToString("yyyy-MM-dd HH:mm") ?? "NULL", task.Status);
            }

            return new TaskDashboardViewModel
            {
                PendingCount = tasksList.Count(t => t.Status == TaskStatus.ToDo && !overdueTaskIds.Contains(t.Id)),
                OngoingCount = tasksList.Count(t => t.Status == TaskStatus.Ongoing && !overdueTaskIds.Contains(t.Id)),
                CompletedCount = tasksList.Count(t => t.Status == TaskStatus.Completed),
                OverdueCount = overdueTasks.Count,
                TotalCount = tasksList.Count,
                ToDoTasks = tasksList.Where(t => t.Status == TaskStatus.ToDo && !overdueTaskIds.Contains(t.Id)).Select(MapToListItem).ToList(),
                OngoingTasks = tasksList.Where(t => t.Status == TaskStatus.Ongoing && !overdueTaskIds.Contains(t.Id)).Select(MapToListItem).ToList(),
                CompletedTasks = tasksList.Where(t => t.Status == TaskStatus.Completed).Take(10).Select(MapToListItem).ToList(),
                OverdueTasks = overdueTasks.Select(MapToListItem).ToList()
            };
        }

        /// <summary>
        /// Gets filtered list of tasks
        /// </summary>
        public async Task<List<TaskListItemViewModel>> GetTasksAsync(string userId, TaskStatus? status = null)
        {
            IEnumerable<TaskItem> tasks;

            if (status.HasValue)
            {
                tasks = await _unitOfWork.Tasks.GetUserTasksByStatusAsync(userId, status.Value);
            }
            else
            {
                tasks = await _unitOfWork.Tasks.GetUserTasksAsync(userId);
            }

            return tasks.Select(MapToListItem).ToList();
        }

        /// <summary>
        /// Gets a task for editing
        /// </summary>
        public async Task<EditTaskViewModel?> GetTaskForEditAsync(int id, string userId)
        {
            var task = await _unitOfWork.Tasks.GetTaskByIdAndUserAsync(id, userId);
            if (task == null) return null;

            return new EditTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Status = task.Status
            };
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public async Task<bool> CreateTaskAsync(CreateTaskViewModel model, string userId)
        {
            try
            {
                // Validate deadline is not in the past
                if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
                {
                    _logger.LogWarning("Attempted to create task with past deadline");
                    return false;
                }

                var task = new TaskItem
                {
                    Title = model.Title,
                    Description = model.Description,
                    Deadline = model.Deadline,
                    Status = TaskStatus.ToDo,
                    UserId = userId
                };

                _logger.LogInformation("Creating task: Title={Title}, Deadline={Deadline}, UserId={UserId}", 
                    task.Title, task.Deadline?.ToString("yyyy-MM-dd HH:mm") ?? "NULL", userId);

                await _unitOfWork.Tasks.AddAsync(task);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Task created successfully: Id={Id}, Title={Title}, Deadline={Deadline}", 
                    task.Id, task.Title, task.Deadline?.ToString("yyyy-MM-dd HH:mm") ?? "NULL");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing task
        /// </summary>
        public async Task<bool> UpdateTaskAsync(EditTaskViewModel model, string userId)
        {
            try
            {
                var task = await _unitOfWork.Tasks.GetTaskByIdAndUserAsync(model.Id, userId);
                if (task == null) return false;

                _logger.LogInformation("Updating task {TaskId}: Old Deadline={OldDeadline}, New Deadline={NewDeadline}", 
                    model.Id, task.Deadline?.ToString("yyyy-MM-dd HH:mm") ?? "NULL", model.Deadline?.ToString("yyyy-MM-dd HH:mm") ?? "NULL");

                // Completed tasks cannot be edited
                if (task.Status == TaskStatus.Completed)
                {
                    _logger.LogWarning("Attempted to edit completed task {TaskId}", model.Id);
                    return false;
                }

                // Validate deadline is not in the past (for new deadlines)
                if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now && task.Deadline != model.Deadline)
                {
                    _logger.LogWarning("Attempted to set past deadline for task {TaskId}", model.Id);
                    return false;
                }

                task.Title = model.Title;
                task.Description = model.Description;
                task.Deadline = model.Deadline;
                task.Status = model.Status;

                if (model.Status == TaskStatus.Completed && !task.CompletedAt.HasValue)
                {
                    task.CompletedAt = DateTime.Now;
                }

                _unitOfWork.Tasks.Update(task);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Task {TaskId} updated successfully: Deadline={Deadline}", 
                    model.Id, task.Deadline?.ToString("yyyy-MM-dd HH:mm") ?? "NULL");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Updates only the status of a task
        /// </summary>
        public async Task<bool> UpdateTaskStatusAsync(int id, TaskStatus status, string userId)
        {
            try
            {
                var task = await _unitOfWork.Tasks.GetTaskByIdAndUserAsync(id, userId);
                if (task == null) return false;

                // Completed tasks cannot have their status changed
                if (task.Status == TaskStatus.Completed && status != TaskStatus.Completed)
                {
                    _logger.LogWarning("Attempted to change status of completed task {TaskId}", id);
                    return false;
                }

                task.Status = status;

                if (status == TaskStatus.Completed)
                {
                    task.CompletedAt = DateTime.Now;
                }

                _unitOfWork.Tasks.Update(task);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Task {TaskId} status updated to {Status} for user {UserId}", id, status, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status {TaskId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a task
        /// </summary>
        public async Task<bool> DeleteTaskAsync(int id, string userId)
        {
            try
            {
                var task = await _unitOfWork.Tasks.GetTaskByIdAndUserAsync(id, userId);
                if (task == null) return false;

                task.IsDeleted = true;
                task.DeletedAt = DateTime.Now;

                _unitOfWork.Tasks.Update(task);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Task {TaskId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId} for user {UserId}", id, userId);
                return false;
            }
        }

        private TaskListItemViewModel MapToListItem(TaskItem task)
        {
            var now = DateTime.Now;
            var isOverdue = task.Deadline.HasValue && task.Deadline.Value < now && task.Status != TaskStatus.Completed;
            
            return new TaskListItemViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Deadline = task.Deadline, // Ensure deadline is mapped
                CompletedAt = task.CompletedAt,
                IsOverdue = isOverdue,
                CreatedAt = task.CreatedAt
            };
        }
    }
}
