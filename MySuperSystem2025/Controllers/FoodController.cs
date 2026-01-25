using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Food;
using MySuperSystem2025.Services;
using MySuperSystem2025.Services.Interfaces;
using System.Security.Claims;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Controller for Food Tracking functionality.
    /// Handles food entry management and dashboard display.
    /// </summary>
    [Authorize]
    public class FoodController : Controller
    {
        private readonly IFoodService _foodService;
        private readonly ILogger<FoodController> _logger;
        private readonly PdfService _pdfService;

        public FoodController(IFoodService foodService, ILogger<FoodController> logger, PdfService pdfService)
        {
            _foodService = foodService;
            _logger = logger;
            _pdfService = pdfService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Dashboard
        [HttpGet]
        public async Task<IActionResult> Index(string? period = null)
        {
            var userId = GetUserId();
            var dashboard = await _foodService.GetDashboardAsync(userId, period);
            return View(dashboard);
        }

        // List all food entries with filters
        [HttpGet]
        public async Task<IActionResult> List(string? period = null, string? mealType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = GetUserId();
            var model = await _foodService.GetFoodEntriesAsync(userId, period, mealType, startDate, endDate);
            return View(model);
        }

        // Create food entry - GET
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MealTypes = MealTypes.All;
            return View(new CreateFoodEntryViewModel());
        }

        // Create food entry - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFoodEntryViewModel model)
        {
            if (model.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MealTypes = MealTypes.All;
                return View(model);
            }

            var userId = GetUserId();
            var success = await _foodService.CreateFoodEntryAsync(model, userId);

            if (success)
            {
                TempData["Success"] = "Food entry added successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to add food entry.";
            ViewBag.MealTypes = MealTypes.All;
            return View(model);
        }

        // Edit food entry - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var model = await _foodService.GetFoodEntryForEditAsync(id, userId);

            if (model == null)
            {
                TempData["Error"] = "Food entry not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MealTypes = MealTypes.All;
            return View(model);
        }

        // Edit food entry - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditFoodEntryViewModel model)
        {
            if (model.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MealTypes = MealTypes.All;
                return View(model);
            }

            var userId = GetUserId();
            var success = await _foodService.UpdateFoodEntryAsync(model, userId);

            if (success)
            {
                TempData["Success"] = "Food entry updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update food entry.";
            ViewBag.MealTypes = MealTypes.All;
            return View(model);
        }

        // Delete food entry - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var model = await _foodService.GetFoodEntryDetailsAsync(id, userId);

            if (model == null)
            {
                TempData["Error"] = "Food entry not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Delete food entry - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var success = await _foodService.DeleteFoodEntryAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Food entry deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete food entry.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Export PDF
        [HttpGet]
        public async Task<IActionResult> ExportPdf(string? period = null)
        {
            var userId = GetUserId();
            var dashboard = await _foodService.GetDashboardAsync(userId, period);
            var entries = await _foodService.GetFoodEntriesAsync(userId, period);

            var pdfBytes = _pdfService.GenerateFoodDashboardPdf(dashboard, entries, User.Identity?.Name ?? "User");

            return File(pdfBytes, "application/pdf", $"FoodTracker_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
