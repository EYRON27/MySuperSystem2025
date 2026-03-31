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
            var visibleCategories = categories.Where(c => !c.IsHidden).ToList();
            var model = new CreateExpenseViewModel
            {
                Date = DateTime.Today,
                Categories = new SelectList(visibleCategories, "Id", "Name")
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
                model.Categories = new SelectList(categories.Where(c => !c.IsHidden), "Id", "Name");
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
            model.Categories = new SelectList(cats.Where(c => !c.IsHidden), "Id", "Name");
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

            // If this is a funds-added entry, redirect to the EditFunds action
            if (expense.Amount < 0)
            {
                return RedirectToAction(nameof(EditFunds), new { id });
            }

            var categories = await _expenseService.GetCategoriesAsync(UserId);
            // Show visible categories + the currently selected one (even if hidden)
            var availableCategories = categories.Where(c => !c.IsHidden || c.Id == expense.CategoryId).ToList();
            expense.Categories = new SelectList(availableCategories, "Id", "Name");
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
                model.Categories = new SelectList(categories.Where(c => !c.IsHidden || c.Id == model.CategoryId), "Id", "Name");
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
            model.Categories = new SelectList(cats.Where(c => !c.IsHidden || c.Id == model.CategoryId), "Id", "Name");
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

        // POST: /Expense/ToggleHideCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHideCategory(int id, string? returnUrl = null)
        {
            var result = await _expenseService.ToggleCategoryHiddenAsync(id, UserId);
            if (result)
            {
                TempData["Success"] = "Category visibility updated.";
            }
            else
            {
                TempData["Error"] = "Failed to update category visibility.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Categories));
        }

        // GET: /Expense/ExportPdf
        public async Task<IActionResult> ExportPdf(string? breakdown = null, string? month = null)
        {
            // Parse month parameter (format: "yyyy-MM") to get date range
            DateTime? startDate = null;
            DateTime? endDate = null;
            string monthLabel = "All";

            if (!string.IsNullOrEmpty(month) && month.Contains('-'))
            {
                var parts = month.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var year) && int.TryParse(parts[1], out var m))
                {
                    startDate = new DateTime(year, m, 1);
                    endDate = startDate.Value.AddMonths(1).AddDays(-1);
                    monthLabel = startDate.Value.ToString("MMMM yyyy");
                }
            }

            var dashboard = await _expenseService.GetDashboardAsync(UserId, breakdown, month);

            // Fetch ALL expenses for the selected month (not just recent 10)
            var expenseList = await _expenseService.GetExpensesAsync(UserId, startDate: startDate, endDate: endDate);

            var pdfBytes = _pdfService.GenerateExpenseDashboardPdf(dashboard, expenseList.Expenses, monthLabel, User.Identity?.Name ?? "User");

            var fileName = startDate.HasValue
                ? $"ExpenseReport_{startDate.Value:yyyyMM}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                : $"ExpenseReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: /Expense/Savings
        public async Task<IActionResult> Savings()
        {
            var model = await _expenseService.GetMonthlySavingsAsync(UserId);
            return View(model);
        }

        // GET: /Expense/AddFunds/5
        [HttpGet]
        public async Task<IActionResult> AddFunds(int id)
        {
            var category = await _expenseService.GetCategoryForEditAsync(id, UserId);
            if (category == null)
            {
                return NotFound();
            }

            // Only allow for one-time budget categories
            if (category.MonthlyFixedBudget > 0)
            {
                TempData["Error"] = "Cannot add funds to a monthly budget category.";
                return RedirectToAction(nameof(Categories));
            }

            var model = new AddFundsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CurrentBudget = category.BudgetAmount,
                CurrentRemaining = category.RemainingAmount,
                Date = DateTime.Today
            };

            return View(model);
        }

        // POST: /Expense/AddFunds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFunds(AddFundsViewModel model)
        {
            if (model.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Date", "Date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                var category = await _expenseService.GetCategoryForEditAsync(model.CategoryId, UserId);
                if (category != null)
                {
                    model.CategoryName = category.Name;
                    model.CurrentBudget = category.BudgetAmount;
                    model.CurrentRemaining = category.RemainingAmount;
                }
                return View(model);
            }

            var result = await _expenseService.AddFundsToCategoryAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = $"₱{model.Amount:N2} added to {model.CategoryName} successfully.";
                return RedirectToAction(nameof(Categories));
            }

            TempData["Error"] = "Failed to add funds.";
            return View(model);
        }

        // GET: /Expense/EditFunds/5
        [HttpGet]
        public async Task<IActionResult> EditFunds(int id)
        {
            var expense = await _expenseService.GetExpenseForEditAsync(id, UserId);
            if (expense == null)
            {
                return NotFound();
            }

            // Only allow editing funds-added entries (negative amounts)
            if (expense.Amount >= 0)
            {
                return RedirectToAction(nameof(Edit), new { id });
            }

            var category = await _expenseService.GetCategoryForEditAsync(expense.CategoryId, UserId);

            var model = new EditFundsViewModel
            {
                Id = expense.Id,
                Amount = -expense.Amount, // Show as positive for the user
                Date = expense.Date,
                Reason = expense.Reason.Replace("[FUNDS ADDED] ", ""),
                CategoryId = expense.CategoryId,
                CategoryName = category?.Name ?? "Unknown"
            };

            return View(model);
        }

        // POST: /Expense/EditFunds/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFunds(int id, EditFundsViewModel model)
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
                return View(model);
            }

            var result = await _expenseService.UpdateFundsEntryAsync(model, UserId);
            if (result)
            {
                TempData["Success"] = "Funds entry updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update funds entry.";
            return View(model);
        }
    }
}
