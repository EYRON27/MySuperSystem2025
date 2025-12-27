using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Expense;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services.Interfaces;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Expense service implementation handling all expense-related business logic
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(IUnitOfWork unitOfWork, ILogger<ExpenseService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data with expense summaries
        /// </summary>
        public async Task<ExpenseDashboardViewModel> GetDashboardAsync(string userId)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            var allExpenses = await _unitOfWork.Expenses.GetUserExpensesAsync(userId);
            var expensesList = allExpenses.ToList();

            var todayExpenses = expensesList.Where(e => e.Date.Date == today).ToList();
            var weeklyExpenses = expensesList.Where(e => e.Date >= startOfWeek && e.Date <= today).ToList();
            var monthlyExpenses = expensesList.Where(e => e.Date >= startOfMonth && e.Date <= today).ToList();
            var yearlyExpenses = expensesList.Where(e => e.Date >= startOfYear && e.Date <= today).ToList();

            // Category breakdown for monthly expenses
            var categoryBreakdown = monthlyExpenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .Select(g => new CategorySummaryViewModel
                {
                    CategoryName = g.Key,
                    Total = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            var monthlyTotal = monthlyExpenses.Sum(e => e.Amount);
            foreach (var category in categoryBreakdown)
            {
                category.Percentage = monthlyTotal > 0 
                    ? Math.Round((category.Total / monthlyTotal) * 100, 1) 
                    : 0;
            }

            return new ExpenseDashboardViewModel
            {
                TodayTotal = todayExpenses.Sum(e => e.Amount),
                WeeklyTotal = weeklyExpenses.Sum(e => e.Amount),
                MonthlyTotal = monthlyTotal,
                YearlyTotal = yearlyExpenses.Sum(e => e.Amount),
                TodayCount = todayExpenses.Count,
                WeeklyCount = weeklyExpenses.Count,
                MonthlyCount = monthlyExpenses.Count,
                YearlyCount = yearlyExpenses.Count,
                RecentExpenses = expensesList.Take(10).Select(MapToListItem).ToList(),
                CategoryBreakdown = categoryBreakdown
            };
        }

        /// <summary>
        /// Gets filtered list of expenses
        /// </summary>
        public async Task<ExpenseListViewModel> GetExpensesAsync(string userId, string? period = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            IEnumerable<Expense> expenses;
            var today = DateTime.Today;

            // Determine date range based on period
            if (!string.IsNullOrEmpty(period))
            {
                (startDate, endDate) = period.ToLower() switch
                {
                    "daily" => (today, today),
                    "weekly" => (today.AddDays(-(int)today.DayOfWeek), today),
                    "monthly" => (new DateTime(today.Year, today.Month, 1), today),
                    "yearly" => (new DateTime(today.Year, 1, 1), today),
                    _ => (startDate, endDate)
                };
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                expenses = await _unitOfWork.Expenses.GetUserExpensesByDateRangeAsync(userId, startDate.Value, endDate.Value);
            }
            else if (categoryId.HasValue)
            {
                expenses = await _unitOfWork.Expenses.GetUserExpensesByCategoryAsync(userId, categoryId.Value);
            }
            else
            {
                expenses = await _unitOfWork.Expenses.GetUserExpensesAsync(userId);
            }

            // Apply category filter if specified
            if (categoryId.HasValue)
            {
                expenses = expenses.Where(e => e.CategoryId == categoryId.Value);
            }

            var expensesList = expenses.ToList();
            var categories = await GetCategoriesAsync(userId);

            return new ExpenseListViewModel
            {
                Expenses = expensesList.Select(MapToListItem).ToList(),
                FilterPeriod = period,
                FilterCategoryId = categoryId,
                FilterStartDate = startDate,
                FilterEndDate = endDate,
                TotalAmount = expensesList.Sum(e => e.Amount),
                TotalCount = expensesList.Count,
                Categories = categories
            };
        }

        /// <summary>
        /// Gets an expense for editing
        /// </summary>
        public async Task<EditExpenseViewModel?> GetExpenseForEditAsync(int id, string userId)
        {
            var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(id, userId);
            if (expense == null) return null;

            return new EditExpenseViewModel
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Date = expense.Date,
                Reason = expense.Reason,
                CategoryId = expense.CategoryId
            };
        }

        /// <summary>
        /// Creates a new expense
        /// </summary>
        public async Task<bool> CreateExpenseAsync(CreateExpenseViewModel model, string userId)
        {
            try
            {
                // Validate date is not in the future
                if (model.Date.Date > DateTime.Today)
                {
                    _logger.LogWarning("Attempted to create expense with future date");
                    return false;
                }

                var expense = new Expense
                {
                    Amount = model.Amount,
                    Date = model.Date,
                    Reason = model.Reason,
                    CategoryId = model.CategoryId,
                    UserId = userId
                };

                await _unitOfWork.Expenses.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense created successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing expense
        /// </summary>
        public async Task<bool> UpdateExpenseAsync(EditExpenseViewModel model, string userId)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(model.Id, userId);
                if (expense == null) return false;

                // Validate date is not in the future
                if (model.Date.Date > DateTime.Today)
                {
                    _logger.LogWarning("Attempted to update expense with future date");
                    return false;
                }

                expense.Amount = model.Amount;
                expense.Date = model.Date;
                expense.Reason = model.Reason;
                expense.CategoryId = model.CategoryId;

                _unitOfWork.Expenses.Update(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense {ExpenseId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense {ExpenseId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes an expense
        /// </summary>
        public async Task<bool> DeleteExpenseAsync(int id, string userId)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(id, userId);
                if (expense == null) return false;

                // Soft delete
                expense.IsDeleted = true;
                expense.DeletedAt = DateTime.UtcNow;

                _unitOfWork.Expenses.Update(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense {ExpenseId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense {ExpenseId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Gets all categories for a user
        /// </summary>
        public async Task<List<ExpenseCategoryViewModel>> GetCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
            return categories.Select(c => new ExpenseCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsDefault = c.IsDefault,
                ExpenseCount = c.Expenses.Count
            }).ToList();
        }

        /// <summary>
        /// Gets a category for editing
        /// </summary>
        public async Task<EditExpenseCategoryViewModel?> GetCategoryForEditAsync(int id, string userId)
        {
            var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(id, userId);
            if (category == null) return null;

            return new EditExpenseCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault
            };
        }

        /// <summary>
        /// Creates a new expense category
        /// </summary>
        public async Task<bool> CreateCategoryAsync(CreateExpenseCategoryViewModel model, string userId)
        {
            try
            {
                // Check if category name already exists
                if (await _unitOfWork.ExpenseCategories.CategoryNameExistsAsync(userId, model.Name))
                {
                    _logger.LogWarning("Category name already exists for user {UserId}", userId);
                    return false;
                }

                var category = new ExpenseCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    UserId = userId,
                    IsDefault = false
                };

                await _unitOfWork.ExpenseCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense category created for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense category for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an expense category
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(EditExpenseCategoryViewModel model, string userId)
        {
            try
            {
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.Id, userId);
                if (category == null) return false;

                // Check if category name already exists (excluding current)
                if (await _unitOfWork.ExpenseCategories.CategoryNameExistsAsync(userId, model.Name, model.Id))
                {
                    _logger.LogWarning("Category name already exists for user {UserId}", userId);
                    return false;
                }

                category.Name = model.Name;
                category.Description = model.Description;

                _unitOfWork.ExpenseCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense category {CategoryId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense category {CategoryId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes an expense category (only if no expenses are associated)
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            try
            {
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(id, userId);
                if (category == null) return false;

                // Cannot delete default categories
                if (category.IsDefault)
                {
                    _logger.LogWarning("Cannot delete default category {CategoryId}", id);
                    return false;
                }

                // Cannot delete if expenses exist
                if (category.Expenses.Any())
                {
                    _logger.LogWarning("Cannot delete category {CategoryId} with existing expenses", id);
                    return false;
                }

                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;

                _unitOfWork.ExpenseCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense category {CategoryId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense category {CategoryId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Seeds default expense categories for a new user
        /// </summary>
        public async Task SeedDefaultCategoriesAsync(string userId)
        {
            var defaultCategories = new[]
            {
                new ExpenseCategory { Name = "Business", Description = "Business related expenses", UserId = userId, IsDefault = true },
                new ExpenseCategory { Name = "Personal", Description = "Personal expenses", UserId = userId, IsDefault = true },
                new ExpenseCategory { Name = "Personal Business", Description = "Personal business expenses", UserId = userId, IsDefault = true }
            };

            foreach (var category in defaultCategories)
            {
                if (!await _unitOfWork.ExpenseCategories.CategoryNameExistsAsync(userId, category.Name))
                {
                    await _unitOfWork.ExpenseCategories.AddAsync(category);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Default expense categories seeded for user {UserId}", userId);
        }

        private static ExpenseListItemViewModel MapToListItem(Expense expense)
        {
            return new ExpenseListItemViewModel
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Date = expense.Date,
                Reason = expense.Reason,
                CategoryName = expense.Category?.Name ?? "Uncategorized",
                CategoryId = expense.CategoryId
            };
        }
    }
}
