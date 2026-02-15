using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Expense;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services.Interfaces;
using System.Globalization;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Expense service implementation handling all expense-related business logic
    /// with balance tracking functionality and monthly budget reset
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpenseService> _logger;

        // Start date for monthly filtering (December 2025)
        private static readonly DateTime BudgetStartDate = new DateTime(2025, 12, 1);

        public ExpenseService(IUnitOfWork unitOfWork, ILogger<ExpenseService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data with expense summaries and balance tracking
        /// </summary>
        public async Task<ExpenseDashboardViewModel> GetDashboardAsync(string userId, string? breakdownPeriod = null, string? selectedMonth = null)
        {
            // First, check and apply monthly budget reset if needed
            await ResetMonthlyBudgetsIfNeededAsync(userId);

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            // Parse selected month or default to current
            int filterYear = today.Year;
            int filterMonth = today.Month;
            if (!string.IsNullOrEmpty(selectedMonth) && selectedMonth.Contains('-'))
            {
                var parts = selectedMonth.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var y) && int.TryParse(parts[1], out var m))
                {
                    filterYear = y;
                    filterMonth = m;
                }
            }

            var filterStartDate = new DateTime(filterYear, filterMonth, 1);
            var filterEndDate = filterStartDate.AddMonths(1).AddDays(-1);

            var allExpenses = await _unitOfWork.Expenses.GetUserExpensesAsync(userId);
            var expensesList = allExpenses.ToList();
            var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
            var categoriesList = categories.ToList();

            var todayExpenses = expensesList.Where(e => e.Date.Date == today).ToList();
            var weeklyExpenses = expensesList.Where(e => e.Date >= startOfWeek && e.Date <= today).ToList();
            var monthlyExpenses = expensesList.Where(e => e.Date >= startOfMonth && e.Date <= today).ToList();
            var yearlyExpenses = expensesList.Where(e => e.Date >= startOfYear && e.Date <= today).ToList();

            // Expenses for the selected/filter month
            var selectedMonthExpenses = expensesList.Where(e => e.Date >= filterStartDate && e.Date <= filterEndDate).ToList();

            // Category breakdown based on selected period OR selected month
            List<Expense> breakdownExpenses;
            string breakdownPeriodName;
            
            if (!string.IsNullOrEmpty(selectedMonth))
            {
                // Use selected month for breakdown
                breakdownExpenses = selectedMonthExpenses;
                breakdownPeriodName = new DateTime(filterYear, filterMonth, 1).ToString("MMMM yyyy");
            }
            else
            {
                switch (breakdownPeriod?.ToLower())
                {
                    case "daily":
                        breakdownExpenses = todayExpenses;
                        breakdownPeriodName = "Today";
                        break;
                    case "weekly":
                        breakdownExpenses = weeklyExpenses;
                        breakdownPeriodName = "This Week";
                        break;
                    case "yearly":
                        breakdownExpenses = yearlyExpenses;
                        breakdownPeriodName = "This Year";
                        break;
                    case "alltime":
                        breakdownExpenses = expensesList;
                        breakdownPeriodName = "All Time";
                        break;
                    case "monthly":
                    default:
                        breakdownExpenses = monthlyExpenses;
                        breakdownPeriodName = "This Month";
                        break;
                }
            }

            var categoryBreakdown = breakdownExpenses
                .GroupBy(e => new { e.CategoryId, Name = e.Category?.Name ?? "Uncategorized" })
                .Select(g => new CategorySummaryViewModel
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Total = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            var breakdownTotal = breakdownExpenses.Sum(e => e.Amount);
            foreach (var category in categoryBreakdown)
            {
                category.Percentage = breakdownTotal > 0 
                    ? Math.Round((category.Total / breakdownTotal) * 100, 1) 
                    : 0;
            }

            // Category balance cards - for selected month
            var categoryBalances = await GetCategoryBalancesForMonthAsync(userId, filterYear, filterMonth);

            // Generate available months (from December 2025 to current)
            var availableMonths = GenerateAvailableMonths(selectedMonth);

            // Calculate totals for selected month
            var selectedMonthTotal = selectedMonthExpenses.Sum(e => e.Amount);
            var totalMonthlyFixedBudget = categoriesList.Sum(c => c.MonthlyFixedBudget);
            
            // FIX: Only count expenses from MONTHLY budget categories for the remaining calculation
            // One-time category expenses should NOT affect the monthly budget remaining
            var monthlyCategoryIds = categoriesList
                .Where(c => c.MonthlyFixedBudget > 0)
                .Select(c => c.Id)
                .ToHashSet();
            var monthlyBudgetSpentThisMonth = selectedMonthExpenses
                .Where(e => monthlyCategoryIds.Contains(e.CategoryId))
                .Sum(e => e.Amount);
            var selectedMonthRemaining = totalMonthlyFixedBudget - monthlyBudgetSpentThisMonth;

            return new ExpenseDashboardViewModel
            {
                TodayTotal = todayExpenses.Sum(e => e.Amount),
                WeeklyTotal = weeklyExpenses.Sum(e => e.Amount),
                MonthlyTotal = monthlyExpenses.Sum(e => e.Amount),
                YearlyTotal = yearlyExpenses.Sum(e => e.Amount),
                TodayCount = todayExpenses.Count,
                WeeklyCount = weeklyExpenses.Count,
                MonthlyCount = monthlyExpenses.Count,
                YearlyCount = yearlyExpenses.Count,
                TotalBudget = categoriesList.Sum(c => c.BudgetAmount),
                TotalRemainingBalance = categoriesList.Sum(c => c.RemainingAmount),
                TotalExpenses = expensesList.Sum(e => e.Amount),
                RecentExpenses = expensesList.Take(10).Select(MapToListItem).ToList(),
                CategoryBreakdown = categoryBreakdown,
                CategoryBalances = categoryBalances,
                BreakdownPeriod = breakdownPeriod ?? "monthly",
                BreakdownPeriodName = breakdownPeriodName,
                SelectedMonth = selectedMonth ?? $"{today.Year}-{today.Month:D2}",
                AvailableMonths = availableMonths,
                TotalMonthlyFixedBudget = totalMonthlyFixedBudget,
                SelectedMonthTotal = selectedMonthTotal,
                MonthlyBudgetSpentThisMonth = monthlyBudgetSpentThisMonth,
                SelectedMonthRemaining = selectedMonthRemaining > 0 ? selectedMonthRemaining : 0
            };
        }

        /// <summary>
        /// Generate list of available months from December 2025 to current month
        /// </summary>
        private List<MonthOption> GenerateAvailableMonths(string? selectedMonth)
        {
            var months = new List<MonthOption>();
            var current = DateTime.Today;
            var date = BudgetStartDate;

            while (date <= current)
            {
                var value = $"{date.Year}-{date.Month:D2}";
                months.Add(new MonthOption
                {
                    Value = value,
                    // Use 3-letter month abbreviation (e.g., "Dec 2025") for the dropdown display
                    Display = date.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                    IsSelected = value == selectedMonth || (string.IsNullOrEmpty(selectedMonth) && date.Year == current.Year && date.Month == current.Month)
                });
                date = date.AddMonths(1);
            }

            // Reverse so most recent is first
            months.Reverse();
            return months;
        }

        /// <summary>
        /// Gets category balances for a specific month
        /// FIX: For one-time budgets, calculate from ALL expenses. For monthly budgets, use month expenses.
        /// Also tracks spent this month for both types (for display purposes).
        /// </summary>
        public async Task<List<CategoryBalanceViewModel>> GetCategoryBalancesForMonthAsync(string userId, int year, int month)
        {
            var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
            var allExpenses = await _unitOfWork.Expenses.GetUserExpensesAsync(userId);
            
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return categories.Select(c =>
            {
                // Always calculate spent this month (for display)
                var spentThisMonth = allExpenses
                    .Where(e => e.CategoryId == c.Id && e.Date >= startOfMonth && e.Date <= endOfMonth)
                    .Sum(e => e.Amount);
                
                decimal budget;
                decimal remaining;
                decimal totalExpensesForBudget;
                
                if (c.MonthlyFixedBudget > 0)
                {
                    // Monthly fixed budget: calculate remaining from current month expenses only
                    budget = c.MonthlyFixedBudget;
                    totalExpensesForBudget = spentThisMonth; // For monthly, total = this month
                    remaining = budget - spentThisMonth;
                }
                else
                {
                    // One-time/manual budget: calculate remaining from ALL expenses (not filtered by date)
                    budget = c.BudgetAmount;
                    totalExpensesForBudget = allExpenses
                        .Where(e => e.CategoryId == c.Id)
                        .Sum(e => e.Amount);
                    remaining = budget - totalExpensesForBudget;
                }

                return new CategoryBalanceViewModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    BudgetAmount = budget,
                    RemainingAmount = remaining > 0 ? remaining : 0,
                    MonthlyFixedBudget = c.MonthlyFixedBudget,
                    SpentThisMonth = spentThisMonth,
                    TotalExpenses = totalExpensesForBudget
                };
            }).ToList();
        }

        /// <summary>
        /// Checks and applies monthly budget reset for all user categories
        /// </summary>
        public async Task ResetMonthlyBudgetsIfNeededAsync(string userId)
        {
            try
            {
                var now = DateTime.Now;
                var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
                var needsSave = false;

                foreach (var category in categories)
                {
                    // Only reset categories with monthly fixed budget
                    if (category.MonthlyFixedBudget <= 0) continue;

                    // Check if needs reset (different year or month)
                    if (category.LastResetYear != now.Year || category.LastResetMonth != now.Month)
                    {
                        // Calculate expenses for current month
                        var startOfMonth = new DateTime(now.Year, now.Month, 1);
                        var currentMonthExpenses = category.Expenses
                            .Where(e => !e.IsDeleted && e.Date >= startOfMonth && e.Date < startOfMonth.AddMonths(1))
                            .Sum(e => e.Amount);

                        // Reset budget to monthly fixed amount
                        category.BudgetAmount = category.MonthlyFixedBudget;
                        category.RemainingAmount = category.MonthlyFixedBudget - currentMonthExpenses;
                        
                        // Ensure remaining doesn't go below 0
                        if (category.RemainingAmount < 0)
                            category.RemainingAmount = 0;

                        // Update last reset tracking
                        category.LastResetYear = now.Year;
                        category.LastResetMonth = now.Month;

                        _unitOfWork.ExpenseCategories.Update(category);
                        needsSave = true;

                        _logger.LogInformation(
                            "Monthly budget reset for category {CategoryName}: Budget={Budget}, Remaining={Remaining}, Month={Month}/{Year}",
                            category.Name, category.BudgetAmount, category.RemainingAmount, now.Month, now.Year);
                    }
                }

                if (needsSave)
                {
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting monthly budgets for user {UserId}", userId);
            }
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
        /// Gets expense details for display/deletion
        /// </summary>
        public async Task<ExpenseListItemViewModel?> GetExpenseDetailsAsync(int id, string userId)
        {
            var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(id, userId);
            if (expense == null) return null;

            return MapToListItem(expense);
        }

        /// <summary>
        /// Creates a new expense. 
        /// For MONTHLY budgets: deducts from stored RemainingAmount
        /// For ONE-TIME budgets: just records the expense (no deduction - calculated dynamically)
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

                // Get the category
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.CategoryId, userId);
                if (category == null)
                {
                    _logger.LogWarning("Category not found for expense creation");
                    return false;
                }

                // Create the expense record (always)
                var expense = new Expense
                {
                    Amount = model.Amount,
                    Date = model.Date,
                    Reason = model.Reason,
                    CategoryId = model.CategoryId,
                    UserId = userId
                };

                // Only deduct from RemainingAmount for MONTHLY budgets
                // One-time budgets: remaining is calculated dynamically, not stored
                if (category.MonthlyFixedBudget > 0)
                {
                    // Check if there's sufficient balance for monthly budgets
                    if (category.RemainingAmount < model.Amount)
                    {
                        _logger.LogWarning("Insufficient balance in monthly budget category {CategoryId} for expense amount {Amount}", model.CategoryId, model.Amount);
                        return false;
                    }
                    
                    category.RemainingAmount -= model.Amount;
                    _unitOfWork.ExpenseCategories.Update(category);
                    _logger.LogInformation("Expense deducted from monthly budget category {CategoryId}", model.CategoryId);
                }
                // For one-time budgets: no deduction needed - just record the expense
                // The remaining balance is calculated dynamically on dashboard load

                await _unitOfWork.Expenses.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense created successfully for user {UserId} in category {CategoryId}", userId, model.CategoryId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing expense.
        /// For MONTHLY budgets: adjusts stored RemainingAmount
        /// For ONE-TIME budgets: just updates the expense record (no balance adjustment)
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

                var oldAmount = expense.Amount;
                var oldCategoryId = expense.CategoryId;
                var amountDifference = model.Amount - oldAmount;

                // Handle category change
                if (oldCategoryId != model.CategoryId)
                {
                    // Refund old category (only if it has monthly budget)
                    var oldCategory = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(oldCategoryId, userId);
                    if (oldCategory != null && oldCategory.MonthlyFixedBudget > 0)
                    {
                        oldCategory.RemainingAmount += oldAmount;
                        _unitOfWork.ExpenseCategories.Update(oldCategory);
                    }

                    // Deduct from new category (only if it has monthly budget)
                    var newCategory = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.CategoryId, userId);
                    if (newCategory == null)
                    {
                        _logger.LogWarning("New category not found for expense update");
                        return false;
                    }

                    if (newCategory.MonthlyFixedBudget > 0)
                    {
                        // Check balance for monthly budgets
                        if (newCategory.RemainingAmount < model.Amount)
                        {
                            _logger.LogWarning("Insufficient balance in new monthly budget category {CategoryId}", model.CategoryId);
                            return false;
                        }
                        newCategory.RemainingAmount -= model.Amount;
                        _unitOfWork.ExpenseCategories.Update(newCategory);
                    }
                }
                else if (amountDifference != 0)
                {
                    // Same category, different amount - only adjust for monthly budgets
                    var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.CategoryId, userId);
                    if (category != null && category.MonthlyFixedBudget > 0)
                    {
                        // Check balance if amount is increasing
                        if (amountDifference > 0 && category.RemainingAmount < amountDifference)
                        {
                            _logger.LogWarning("Insufficient balance for expense increase in monthly budget category {CategoryId}", model.CategoryId);
                            return false;
                        }
                        category.RemainingAmount -= amountDifference;
                        _unitOfWork.ExpenseCategories.Update(category);
                    }
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
        /// Soft deletes an expense.
        /// For MONTHLY budgets: refunds the amount to stored RemainingAmount
        /// For ONE-TIME budgets: just marks as deleted (no balance adjustment)
        /// </summary>
        public async Task<bool> DeleteExpenseAsync(int id, string userId)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(id, userId);
                if (expense == null) return false;

                // Refund the amount only for MONTHLY budget categories
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(expense.CategoryId, userId);
                if (category != null && category.MonthlyFixedBudget > 0)
                {
                    category.RemainingAmount += expense.Amount;
                    _unitOfWork.ExpenseCategories.Update(category);
                    _logger.LogInformation("Expense amount refunded to monthly budget category {CategoryId}", expense.CategoryId);
                }

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
        /// Gets all categories for a user with dynamically calculated balance information
        /// FIX: Calculate remaining dynamically based on budget type instead of using stored value
        /// </summary>
        public async Task<List<ExpenseCategoryViewModel>> GetCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
            var allExpenses = await _unitOfWork.Expenses.GetUserExpensesAsync(userId);
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            
            return categories.Select(c => 
            {
                decimal remaining;
                
                if (c.MonthlyFixedBudget > 0)
                {
                    // Monthly budget: calculate from current month expenses only
                    var monthExpenses = allExpenses
                        .Where(e => e.CategoryId == c.Id && e.Date >= startOfMonth && e.Date < endOfMonth)
                        .Sum(e => e.Amount);
                    remaining = c.MonthlyFixedBudget - monthExpenses;
                }
                else if (c.BudgetAmount > 0)
                {
                    // One-time budget: calculate from ALL expenses
                    var totalExpenses = allExpenses
                        .Where(e => e.CategoryId == c.Id)
                        .Sum(e => e.Amount);
                    remaining = c.BudgetAmount - totalExpenses;
                }
                else
                {
                    remaining = 0;
                }
                
                return new ExpenseCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsDefault = c.IsDefault,
                    ExpenseCount = c.Expenses.Count,
                    BudgetAmount = c.MonthlyFixedBudget > 0 ? c.MonthlyFixedBudget : c.BudgetAmount,
                    RemainingAmount = remaining > 0 ? remaining : 0,
                    MonthlyFixedBudget = c.MonthlyFixedBudget
                };
            }).ToList();
        }

        /// <summary>
        /// Gets a category for editing with dynamically calculated balance info
        /// FIX: Calculate remaining dynamically based on budget type
        /// </summary>
        public async Task<EditExpenseCategoryViewModel?> GetCategoryForEditAsync(int id, string userId)
        {
            var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(id, userId);
            if (category == null) return null;

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            
            decimal totalExpenses;
            decimal budget;
            
            if (category.MonthlyFixedBudget > 0)
            {
                // Monthly budget: calculate from current month expenses only
                budget = category.MonthlyFixedBudget;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted && e.Date >= startOfMonth && e.Date < endOfMonth)
                    .Sum(e => e.Amount);
            }
            else
            {
                // One-time budget: calculate from ALL expenses
                budget = category.BudgetAmount;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted)
                    .Sum(e => e.Amount);
            }
            
            var remaining = budget - totalExpenses;

            return new EditExpenseCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault,
                BudgetAmount = budget,
                RemainingAmount = remaining > 0 ? remaining : 0,
                TotalExpenses = totalExpenses,
                MonthlyFixedBudget = category.MonthlyFixedBudget
            };
        }

        /// <summary>
        /// Gets category details for display/deletion with dynamically calculated balance
        /// FIX: Calculate remaining dynamically based on budget type
        /// </summary>
        public async Task<ExpenseCategoryViewModel?> GetCategoryDetailsAsync(int id, string userId)
        {
            var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(id, userId);
            if (category == null) return null;

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            
            decimal totalExpenses;
            decimal budget;
            
            if (category.MonthlyFixedBudget > 0)
            {
                // Monthly budget: calculate from current month expenses only
                budget = category.MonthlyFixedBudget;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted && e.Date >= startOfMonth && e.Date < endOfMonth)
                    .Sum(e => e.Amount);
            }
            else
            {
                // One-time budget: calculate from ALL expenses
                budget = category.BudgetAmount;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted)
                    .Sum(e => e.Amount);
            }
            
            var remaining = budget - totalExpenses;

            return new ExpenseCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault,
                ExpenseCount = category.Expenses.Count(e => !e.IsDeleted),
                BudgetAmount = budget,
                RemainingAmount = remaining > 0 ? remaining : 0,
                MonthlyFixedBudget = category.MonthlyFixedBudget
            };
        }

        /// <summary>
        /// Creates a new expense category with optional budget and monthly fixed budget
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

                var now = DateTime.Now;
                var budgetAmount = model.MonthlyFixedBudget > 0 ? model.MonthlyFixedBudget : model.BudgetAmount;

                var category = new ExpenseCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    UserId = userId,
                    IsDefault = false,
                    BudgetAmount = budgetAmount,
                    RemainingAmount = budgetAmount, // Initially, remaining = budget
                    MonthlyFixedBudget = model.MonthlyFixedBudget,
                    LastResetYear = model.MonthlyFixedBudget > 0 ? now.Year : null,
                    LastResetMonth = model.MonthlyFixedBudget > 0 ? now.Month : null
                };

                await _unitOfWork.ExpenseCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense category created for user {UserId} with budget {Budget}, monthly fixed {MonthlyFixed}", 
                    userId, model.BudgetAmount, model.MonthlyFixedBudget);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense category for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an expense category including budget and monthly fixed budget adjustment
        /// FIX: For one-time budgets, calculate from ALL expenses. For monthly budgets, use current month expenses.
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

                var now = DateTime.Now;
                
                category.Name = model.Name;
                category.Description = model.Description;
                category.MonthlyFixedBudget = model.MonthlyFixedBudget;

                // Determine budget amount
                var newBudget = model.MonthlyFixedBudget > 0 ? model.MonthlyFixedBudget : model.BudgetAmount;
                category.BudgetAmount = newBudget;

                // FIX: Calculate remaining based on budget type
                if (newBudget > 0)
                {
                    decimal totalExpenses;
                    
                    if (model.MonthlyFixedBudget > 0)
                    {
                        // Monthly fixed budget: calculate from current month expenses only
                        var startOfMonth = new DateTime(now.Year, now.Month, 1);
                        totalExpenses = category.Expenses
                            .Where(e => !e.IsDeleted && e.Date >= startOfMonth && e.Date < startOfMonth.AddMonths(1))
                            .Sum(e => e.Amount);
                    }
                    else
                    {
                        // One-time/manual budget: calculate from ALL expenses (not filtered by date)
                        totalExpenses = category.Expenses
                            .Where(e => !e.IsDeleted)
                            .Sum(e => e.Amount);
                    }
                    
                    category.RemainingAmount = newBudget - totalExpenses;
                    
                    // Clamp to 0 for display (don't show negative)
                    if (category.RemainingAmount < 0)
                        category.RemainingAmount = 0;
                }
                else
                {
                    category.RemainingAmount = 0;
                }

                // Update reset tracking if monthly budget is set
                if (model.MonthlyFixedBudget > 0)
                {
                    category.LastResetYear = now.Year;
                    category.LastResetMonth = now.Month;
                }

                _unitOfWork.ExpenseCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense category {CategoryId} updated for user {UserId}: Budget={Budget}, Remaining={Remaining}", 
                    model.Id, userId, newBudget, category.RemainingAmount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense category {CategoryId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes an expense category (allows deletion even with expenses)
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            try
            {
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(id, userId);
                if (category == null) return false;

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
                new ExpenseCategory { Name = "Business", Description = "Business related expenses", UserId = userId, IsDefault = true, BudgetAmount = 0, RemainingAmount = 0 },
                new ExpenseCategory { Name = "Personal", Description = "Personal expenses", UserId = userId, IsDefault = true, BudgetAmount = 0, RemainingAmount = 0 },
                new ExpenseCategory { Name = "Personal Business", Description = "Personal business expenses", UserId = userId, IsDefault = true, BudgetAmount = 0, RemainingAmount = 0 }
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

        /// <summary>
        /// Sets budget for a category and calculates remaining based on existing expenses
        /// </summary>
        public async Task<bool> SetCategoryBudgetAsync(int categoryId, decimal budgetAmount, string userId)
        {
            try
            {
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(categoryId, userId);
                if (category == null) return false;

                // Calculate total expenses for this category
                var totalExpenses = category.Expenses.Where(e => !e.IsDeleted).Sum(e => e.Amount);

                category.BudgetAmount = budgetAmount;
                category.RemainingAmount = budgetAmount - totalExpenses;

                // Ensure remaining doesn't go below 0
                if (category.RemainingAmount < 0)
                {
                    category.RemainingAmount = 0;
                }

                _unitOfWork.ExpenseCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Budget set for category {CategoryId}: Budget={Budget}, Remaining={Remaining}", 
                    categoryId, budgetAmount, category.RemainingAmount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting budget for category {CategoryId}", categoryId);
                return false;
            }
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
