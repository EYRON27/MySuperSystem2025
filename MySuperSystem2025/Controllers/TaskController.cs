using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySuperSystem2025.Models.ViewModels.Task;
using MySuperSystem2025.Services.Interfaces;
using TaskStatus = MySuperSystem2025.Models.Domain.TaskStatus;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Task controller handling task/to-do management functionality
    /// </summary>
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskService taskService, ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Task
        public async Task<IActionResult> Index()
        {
            var dashboard = await _taskService.GetDashboardAsync(UserId);
            return View(dashboard);
        }

        // GET: /Task/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTaskViewModel());
        }

        // POST: /Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (model.Deadline.HasValue && model.Deadline.Value < DateTime.Now)
            {
                ModelState.AddModelError("Deadline", "Deadline cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _taskService.CreateTaskAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Task created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to create task.";
            return View(model);
        }

        // GET: /Task/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskForEditAsync(id, UserId);
            if (task == null)
            {
                return NotFound();
            }

            // Completed tasks are read-only
            if (task.IsCompleted)
            {
                return View("Details", task);
            }

            return View(task);
        }

        // POST: /Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTaskViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            // Cannot edit completed tasks
            if (model.Status == TaskStatus.Completed)
            {
                TempData["Error"] = "Completed tasks cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _taskService.UpdateTaskAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Task updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update task.";
            return View(model);
        }

        // POST: /Task/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TaskStatus status)
        {
            var result = await _taskService.UpdateTaskStatusAsync(id, status, UserId);
            if (result)
            {
                TempData["Success"] = "Task status updated.";
            }
            else
            {
                TempData["Error"] = "Failed to update task status.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Task/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetTaskForEditAsync(id, UserId);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: /Task/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id, UserId);
            if (result)
            {
                TempData["Success"] = "Task deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete task.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Task/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetTaskForEditAsync(id, UserId);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }
    }
}
