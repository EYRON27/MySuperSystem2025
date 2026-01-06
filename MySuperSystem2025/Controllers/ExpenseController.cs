using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySuperSystem2025.Models.ViewModels.Expense;
using MySuperSystem2025.Services.Interfaces;
using MySuperSystem2025.Services;

namespace MySuperSystem2025.Controllers
{
    /// <summary>
    /// Expense controller handling expense management functionality
    /// </summary>
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpenseController> _logger;
        private readonly PdfService _pdfService;

        public ExpenseController(IExpenseService expenseService, ILogger<ExpenseController> logger, PdfService pdfService)
        {
            _expenseService = expenseService;
            _logger = logger;
            _pdfService = pdfService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Expense
        public async Task<IActionResult> Index(string? period = null, int? categoryId = null, string? breakdown = null, string? month = null)
        {
            var dashboard = await _expenseService.GetDashboardAsync(UserId, breakdown, month);
            return View(dashboard);
        }

        // GET: /Expense/List
        public async Task<IActionResult> List(string? period = null, int? categoryId = null)
        {
            var expenses = await _expenseService.GetExpensesAsync(UserId, period, categoryId);
            return View(expenses);
        }

        // GET: /Expense/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _expenseService.GetCategoriesAsync(UserId);
            var model = new CreateExpenseViewModel
            {
                Date = DateTime.Today,
                Categories = new SelectList(categories, "Id", "Name")
            };
            return View(model);
        }

        // POST: /Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExpenseViewModel model)
        {
            if (model.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                var categories = await _expenseService.GetCategoriesAsync(UserId);
                model.Categories = new SelectList(categories, "Id", "Name");
                return View(model);
            }

            var result = await _expenseService.CreateExpenseAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Expense created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Check if it's an insufficient balance error
            var category = (await _expenseService.GetCategoriesAsync(UserId))
                .FirstOrDefault(c => c.Id == model.CategoryId);
            if (category != null && category.BudgetAmount > 0 && category.RemainingAmount < model.Amount)
            {
                TempData["Error"] = $"Insufficient balance in {category.Name}. Available: ?{category.RemainingAmount:N2}";
            }
            else
            {
                TempData["Error"] = "Failed to create expense.";
            }
            
            var cats = await _expenseService.GetCategoriesAsync(UserId);
            model.Categories = new SelectList(cats, "Id", "Name");
            return View(model);
        }

        // GET: /Expense/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _expenseService.GetExpenseForEditAsync(id, UserId);
            if (expense == null)
            {
                return NotFound();
            }

            var categories = await _expenseService.GetCategoriesAsync(UserId);
            expense.Categories = new SelectList(categories, "Id", "Name");
            return View(expense);
        }

        // POST: /Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditExpenseViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (model.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                var categories = await _expenseService.GetCategoriesAsync(UserId);
                model.Categories = new SelectList(categories, "Id", "Name");
                return View(model);
            }

            var result = await _expenseService.UpdateExpenseAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Expense updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Check if it's an insufficient balance error
            var category = (await _expenseService.GetCategoriesAsync(UserId))
                .FirstOrDefault(c => c.Id == model.CategoryId);
            if (category != null && category.BudgetAmount > 0)
            {
                TempData["Error"] = $"Insufficient balance in {category.Name}. Available: ?{category.RemainingAmount:N2}";
            }
            else
            {
                TempData["Error"] = "Failed to update expense.";
            }
            
            var cats = await _expenseService.GetCategoriesAsync(UserId);
            model.Categories = new SelectList(cats, "Id", "Name");
            return View(model);
        }

        // GET: /Expense/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _expenseService.GetExpenseForEditAsync(id, UserId);
            if (expense == null)
            {
                return NotFound();
            }

            var expenseItem = await _expenseService.GetExpenseDetailsAsync(id, UserId);
            if (expenseItem == null)
            {
                return NotFound();
            }

            return View(expenseItem);
        }

        // POST: /Expense/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _expenseService.DeleteExpenseAsync(id, UserId);
            if (result)
            {
                TempData["Success"] = "Expense deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete expense.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Expense/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _expenseService.GetCategoriesAsync(UserId);
            return View(categories);
        }

        // GET: /Expense/CreateCategory
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new CreateExpenseCategoryViewModel());
        }

        // POST: /Expense/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateExpenseCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _expenseService.CreateCategoryAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Categories));
            }

            ModelState.AddModelError("Name", "Category name already exists.");
            return View(model);
        }

        // GET: /Expense/EditCategory/5
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _expenseService.GetCategoryForEditAsync(id, UserId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Expense/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, EditExpenseCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _expenseService.UpdateCategoryAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Categories));
            }

            ModelState.AddModelError("Name", "Category name already exists.");
            return View(model);
        }

        // GET: /Expense/DeleteCategory/5
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _expenseService.GetCategoryForEditAsync(id, UserId);
            if (category == null)
            {
                return NotFound();
            }

            var categoryViewModel = await _expenseService.GetCategoryDetailsAsync(id, UserId);
            if (categoryViewModel == null)
            {
                return NotFound();
            }

            return View(categoryViewModel);
        }

        // POST: /Expense/DeleteCategoryConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var result = await _expenseService.DeleteCategoryAsync(id, UserId);
            if (result)
            {
                TempData["Success"] = "Category deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Cannot delete this category. It may have expenses associated.";
            }

            return RedirectToAction(nameof(Categories));
        }

        // GET: /Expense/ExportPdf
        public async Task<IActionResult> ExportPdf(string? breakdown = null)
        {
            var dashboard = await _expenseService.GetDashboardAsync(UserId, breakdown);
            var pdfBytes = _pdfService.GenerateExpenseDashboardPdf(dashboard, User.Identity?.Name ?? "User");

            return File(pdfBytes, "application/pdf", $"ExpenseReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
