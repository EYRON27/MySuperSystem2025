using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySuperSystem2025.Models.ViewModels.Time;
using MySuperSystem2025.Services.Interfaces;
using System.Security.Claims;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Controller for Time Tracking functionality.
    /// Handles time entry management, category management, and dashboard display.
    /// </summary>
    [Authorize]
    public class TimeController : Controller
    {
        private readonly ITimeService _timeService;
        private readonly ILogger<TimeController> _logger;

        public TimeController(ITimeService timeService, ILogger<TimeController> logger)
        {
            _timeService = timeService;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Index(string? period = null)
        {
            var userId = GetUserId();
            var dashboard = await _timeService.GetDashboardAsync(userId, period);
            var categories = await _timeService.GetCategoriesAsync(userId);

            ViewBag.Categories = categories;
            return View(dashboard);
        }

        // List all time entries with filters
        [HttpGet]
        public async Task<IActionResult> List(string? period = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = GetUserId();
            var model = await _timeService.GetTimeEntriesAsync(userId, period, categoryId, startDate, endDate);
            return View(model);
        }

        // Create time entry - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = GetUserId();
            var categories = await _timeService.GetCategoriesAsync(userId);

            if (!categories.Any())
            {
                TempData["Warning"] = "Please create at least one category before adding a time entry.";
                return RedirectToAction(nameof(Categories));
            }

            ViewBag.Categories = categories;
            return View(new CreateTimeEntryViewModel());
        }

        // Create time entry - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTimeEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _timeService.GetCategoriesAsync(GetUserId());
                ViewBag.Categories = categories;
                return View(model);
            }

            var userId = GetUserId();
            var success = await _timeService.CreateTimeEntryAsync(model, userId);

            if (success)
            {
                TempData["Success"] = "Time entry added successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to add time entry. Please check the start and end times.";
            var cats = await _timeService.GetCategoriesAsync(userId);
            ViewBag.Categories = cats;
            return View(model);
        }

        // Edit time entry - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var model = await _timeService.GetTimeEntryForEditAsync(id, userId);

            if (model == null)
            {
                TempData["Error"] = "Time entry not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _timeService.GetCategoriesAsync(userId);
            ViewBag.Categories = categories;
            return View(model);
        }

        // Edit time entry - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTimeEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _timeService.GetCategoriesAsync(GetUserId());
                ViewBag.Categories = categories;
                return View(model);
            }

            var userId = GetUserId();
            var success = await _timeService.UpdateTimeEntryAsync(model, userId);

            if (success)
            {
                TempData["Success"] = "Time entry updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update time entry. Please check the start and end times.";
            var cats = await _timeService.GetCategoriesAsync(userId);
            ViewBag.Categories = cats;
            return View(model);
        }

        // Delete time entry - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var model = await _timeService.GetTimeEntryDetailsAsync(id, userId);

            if (model == null)
            {
                TempData["Error"] = "Time entry not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Delete time entry - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var success = await _timeService.DeleteTimeEntryAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Time entry deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete time entry.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var userId = GetUserId();
            var categories = await _timeService.GetCategoriesAsync(userId);
            return View(categories);
        }

        // Create category - GET
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new CreateTimeCategoryViewModel());
        }

        // Create category - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateTimeCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetUserId();
            var success = await _timeService.CreateCategoryAsync(model, userId);

            if (success)
            {
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Categories));
            }

            TempData["Error"] = "Failed to create category. Category name may already exist.";
            return View(model);
        }

        // Edit category - GET
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var userId = GetUserId();
            var model = await _timeService.GetCategoryForEditAsync(id, userId);

            if (model == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            return View(model);
        }

        // Edit category - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(EditTimeCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetUserId();
            var success = await _timeService.UpdateCategoryAsync(model, userId);

            if (success)
            {
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Categories));
            }

            TempData["Error"] = "Failed to update category. Category name may already exist.";
            return View(model);
        }

        // Delete category - GET
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserId();
            var model = await _timeService.GetCategoryDetailsAsync(id, userId);

            if (model == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            return View(model);
        }

        // Delete category - POST
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var userId = GetUserId();
            var success = await _timeService.DeleteCategoryAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Category deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete category.";
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
