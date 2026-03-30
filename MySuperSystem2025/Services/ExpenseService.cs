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
        /// Recalculates and persists RemainingAmount for a one-time budget category.
        /// Formula: RemainingAmount = BudgetAmount - sum(all non-deleted positive expenses)
        /// Call this after ANY change to expenses in a one-time budget category.
        /// </summary>
        private void RecalcOneTimeBudget(ExpenseCategory category)
        {
            if (category.MonthlyFixedBudget > 0) return; // not a one-time budget
            if (category.BudgetAmount <= 0) return; // no budget set

            var totalSpent = category.Expenses
                .Where(e => !e.IsDeleted && e.Amount > 0)
                .Sum(e => e.Amount);

            category.RemainingAmount = category.BudgetAmount - totalSpent;
            if (category.RemainingAmount < 0)
                category.RemainingAmount = 0;

            _unitOfWork.ExpenseCategories.Update(category);
        }

        /// <summary>
        /// Gets the dynamically-calculated remaining balance for a one-time budget category.
        /// Does NOT touch the database.
        /// </summary>
        private static decimal CalcOneTimeRemaining(ExpenseCategory category)
        {
            if (category.BudgetAmount <= 0) return 0;
            var totalSpent = category.Expenses
                .Where(e => !e.IsDeleted && e.Amount > 0)
                .Sum(e => e.Amount);
            var remaining = category.BudgetAmount - totalSpent;
            return remaining > 0 ? remaining : 0;
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

            // Filter out funds-added entries (negative amounts) for spending totals
            var actualExpenses = expensesList.Where(e => e.Amount > 0).ToList();

            var todayExpenses = actualExpenses.Where(e => e.Date.Date == today).ToList();
            var weeklyExpenses = actualExpenses.Where(e => e.Date >= startOfWeek && e.Date <= today).ToList();
            var monthlyExpenses = actualExpenses.Where(e => e.Date >= startOfMonth && e.Date <= today).ToList();
            var yearlyExpenses = actualExpenses.Where(e => e.Date >= startOfYear && e.Date <= today).ToList();

            // Expenses for the selected/filter month (only actual expenses, not funds-added)
            var selectedMonthExpenses = actualExpenses.Where(e => e.Date >= filterStartDate && e.Date <= filterEndDate).ToList();

            // Category breakdown based on selected period OR selected month (only actual expenses)
            List<Expense> breakdownExpenses;
            string breakdownPeriodName;
            
            if (!string.IsNullOrEmpty(selectedMonth))
            {
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
                        breakdownExpenses = actualExpenses;
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

            // Calculate totals for selected month — only ACTIVE monthly budget categories
            // Include rollover from previous month in the budget total
            var selectedMonthTotal = selectedMonthExpenses.Sum(e => e.Amount);
            
            var prevFilterMonthStart = filterStartDate.AddMonths(-1);
            decimal totalMonthlyFixedBudget = 0;
            var activeMonthlyCats = categoriesList.Where(c => c.MonthlyFixedBudget > 0 && c.IsBudgetActive).ToList();
            foreach (var cat in activeMonthlyCats)
            {
                decimal rollover = 0;
                if (cat.RolloverEnabled && prevFilterMonthStart >= BudgetStartDate)
                {
                    var prevEnd = prevFilterMonthStart.AddMonths(1).AddDays(-1);
                    var prevSpent = actualExpenses
                        .Where(e => e.CategoryId == cat.Id && e.Date >= prevFilterMonthStart && e.Date <= prevEnd)
                        .Sum(e => e.Amount);
                    var leftover = cat.MonthlyFixedBudget - prevSpent;
                    if (leftover > 0) rollover = leftover;
                }
                totalMonthlyFixedBudget += cat.MonthlyFixedBudget + rollover;
            }
            
            // Only count expenses from ACTIVE MONTHLY budget categories for the remaining calculation
            var monthlyCategoryIds = activeMonthlyCats
                .Select(c => c.Id)
                .ToHashSet();
            var monthlyBudgetSpentThisMonth = selectedMonthExpenses
                .Where(e => monthlyCategoryIds.Contains(e.CategoryId))
                .Sum(e => e.Amount);
            var selectedMonthRemaining = totalMonthlyFixedBudget - monthlyBudgetSpentThisMonth;

            // Calculate accumulated savings from PAST months (monthly budget categories only)
            var monthlyCategoriesList = categoriesList.Where(c => c.MonthlyFixedBudget > 0 && c.IsBudgetActive).ToList();
            decimal totalMonthlySavings = 0;

            if (monthlyCategoriesList.Any())
            {
                var savingsDate = BudgetStartDate;
                var selectedMonthStart = new DateTime(filterYear, filterMonth, 1);

                while (savingsDate < selectedMonthStart)
                {
                    var monthStart = new DateTime(savingsDate.Year, savingsDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    foreach (var cat in monthlyCategoriesList)
                    {
                        var spentInMonth = actualExpenses
                            .Where(e => e.CategoryId == cat.Id && e.Date >= monthStart && e.Date <= monthEnd)
                            .Sum(e => e.Amount);

                        var savedThisMonth = cat.MonthlyFixedBudget - spentInMonth;
                        if (savedThisMonth > 0)
                        {
                            totalMonthlySavings += savedThisMonth;
                        }
                    }

                    savingsDate = savingsDate.AddMonths(1);
                }
            }

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
                TotalExpenses = actualExpenses.Sum(e => e.Amount),
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
                SelectedMonthRemaining = selectedMonthRemaining > 0 ? selectedMonthRemaining : 0,
                TotalMonthlySavings = totalMonthlySavings
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
        /// Gets category balances for a specific month.
        /// For monthly budgets: includes rollover from the previous month.
        /// For one-time budgets: dynamically calculates remaining from stored BudgetAmount.
        /// This method is READ-ONLY — it does NOT modify database values.
        /// </summary>
        public async Task<List<CategoryBalanceViewModel>> GetCategoryBalancesForMonthAsync(string userId, int year, int month)
        {
            var categories = (await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId)).ToList();
            var allExpenses = (await _unitOfWork.Expenses.GetUserExpensesAsync(userId)).ToList();
            
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return categories.Select(c =>
            {
                var spentThisMonth = allExpenses
                    .Where(e => e.CategoryId == c.Id && e.Amount > 0 && e.Date >= startOfMonth && e.Date <= endOfMonth)
                    .Sum(e => e.Amount);
                
                decimal budget;
                decimal remaining;
                decimal totalExpensesForBudget;
                
                if (c.MonthlyFixedBudget > 0)
                {
                    decimal rollover = 0;
                    if (c.RolloverEnabled)
                    {
                        var prevMonthStart = startOfMonth.AddMonths(-1);
                        if (prevMonthStart >= BudgetStartDate)
                        {
                            var prevMonthEnd = prevMonthStart.AddMonths(1).AddDays(-1);
                            var prevSpent = allExpenses
                                .Where(e => e.CategoryId == c.Id && e.Amount > 0 && e.Date >= prevMonthStart && e.Date <= prevMonthEnd)
                                .Sum(e => e.Amount);
                            var prevLeftover = c.MonthlyFixedBudget - prevSpent;
                            if (prevLeftover > 0)
                                rollover = prevLeftover;
                        }
                    }

                    budget = c.MonthlyFixedBudget + rollover;
                    totalExpensesForBudget = spentThisMonth;
                    remaining = budget - spentThisMonth;
                }
                else
                {
                    // One-time budget: use stored BudgetAmount as-is (set by user via EditCategory/AddFunds)
                    // Dynamically calculate remaining = BudgetAmount - all positive expenses
                    budget = c.BudgetAmount;
                    totalExpensesForBudget = allExpenses
                        .Where(e => e.CategoryId == c.Id && e.Amount > 0)
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
                    IsBudgetActive = c.IsBudgetActive,
                    SpentThisMonth = spentThisMonth,
                    TotalExpenses = totalExpensesForBudget,
                    IsHidden = c.IsHidden
                };
            }).ToList();
        }

        /// <summary>
        /// Checks and applies monthly budget reset for all user categories.
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
                    if (category.MonthlyFixedBudget <= 0 || !category.IsBudgetActive) continue;

                    if (category.LastResetYear != now.Year || category.LastResetMonth != now.Month)
                    {
                        decimal rollover = 0;
                        if (category.RolloverEnabled && category.LastResetYear.HasValue && category.LastResetMonth.HasValue)
                        {
                            var prevMonthStart = new DateTime(category.LastResetYear.Value, category.LastResetMonth.Value, 1);
                            var prevMonthEnd = prevMonthStart.AddMonths(1).AddDays(-1);
                            var prevMonthExpenses = category.Expenses
                                .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= prevMonthStart && e.Date <= prevMonthEnd)
                                .Sum(e => e.Amount);
                            var prevLeftover = category.MonthlyFixedBudget - prevMonthExpenses;
                            if (prevLeftover > 0)
                                rollover = prevLeftover;
                        }

                        var startOfMonth = new DateTime(now.Year, now.Month, 1);
                        var currentMonthExpenses = category.Expenses
                            .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= startOfMonth && e.Date < startOfMonth.AddMonths(1))
                            .Sum(e => e.Amount);

                        var effectiveBudget = category.MonthlyFixedBudget + rollover;
                        category.BudgetAmount = effectiveBudget;
                        category.RemainingAmount = effectiveBudget - currentMonthExpenses;
                        
                        if (category.RemainingAmount < 0)
                            category.RemainingAmount = 0;

                        category.LastResetYear = now.Year;
                        category.LastResetMonth = now.Month;

                        _unitOfWork.ExpenseCategories.Update(category);
                        needsSave = true;

                        _logger.LogInformation(
                            "Monthly budget reset for category {CategoryName}: Base={Base}, Rollover={Rollover}, EffectiveBudget={Budget}, Remaining={Remaining}, Month={Month}/{Year}",
                            category.Name, category.MonthlyFixedBudget, rollover, effectiveBudget, category.RemainingAmount, now.Month, now.Year);
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
                TotalAmount = expensesList.Where(e => e.Amount > 0).Sum(e => e.Amount),
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
        /// For BOTH monthly and one-time budgets: validates balance and updates stored RemainingAmount.
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

                // Balance check for ANY category with a budget
                if (category.BudgetAmount > 0)
                {
                    decimal currentRemaining;
                    if (category.MonthlyFixedBudget > 0)
                    {
                        currentRemaining = category.RemainingAmount;
                    }
                    else
                    {
                        // One-time: calculate dynamically from loaded expenses
                        currentRemaining = CalcOneTimeRemaining(category);
                    }

                    if (currentRemaining < model.Amount)
                    {
                        _logger.LogWarning("Insufficient balance in category {CategoryId} for expense amount {Amount}. Available: {Available}",
                            model.CategoryId, model.Amount, currentRemaining);
                        return false;
                    }
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

                // Update stored RemainingAmount
                if (category.MonthlyFixedBudget > 0)
                {
                    category.RemainingAmount -= model.Amount;
                    if (category.RemainingAmount < 0) category.RemainingAmount = 0;
                    _unitOfWork.ExpenseCategories.Update(category);
                }
                else if (category.BudgetAmount > 0)
                {
                    // One-time budget: add the new expense to the collection first so recalc sees it
                    category.Expenses.Add(expense);
                    RecalcOneTimeBudget(category);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Expense created for user {UserId} in category {CategoryId}, amount {Amount}", userId, model.CategoryId, model.Amount);
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
        /// For BOTH monthly and one-time budgets: validates balance and updates stored RemainingAmount.
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

                // Category change
                if (oldCategoryId != model.CategoryId)
                {
                    // --- Refund old category ---
                    var oldCategory = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(oldCategoryId, userId);
                    if (oldCategory != null)
                    {
                        if (oldCategory.MonthlyFixedBudget > 0)
                        {
                            oldCategory.RemainingAmount += oldAmount;
                            _unitOfWork.ExpenseCategories.Update(oldCategory);
                        }
                        else if (oldCategory.BudgetAmount > 0)
                        {
                            // Mark old amount as 0 in memory so recalc excludes it, then recalc
                            expense.Amount = 0;
                            RecalcOneTimeBudget(oldCategory);
                        }
                    }

                    // --- Deduct from new category ---
                    var newCategory = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.CategoryId, userId);
                    if (newCategory == null)
                    {
                        _logger.LogWarning("New category not found for expense update");
                        return false;
                    }

                    // Balance check on new category
                    if (newCategory.BudgetAmount > 0)
                    {
                        decimal available;
                        if (newCategory.MonthlyFixedBudget > 0)
                        {
                            available = newCategory.RemainingAmount;
                        }
                        else
                        {
                            available = CalcOneTimeRemaining(newCategory);
                        }

                        if (available < model.Amount)
                        {
                            _logger.LogWarning("Insufficient balance in new category {CategoryId}", model.CategoryId);
                            // Restore old expense amount before returning
                            expense.Amount = oldAmount;
                            return false;
                        }
                    }

                    if (newCategory.MonthlyFixedBudget > 0)
                    {
                        newCategory.RemainingAmount -= model.Amount;
                        if (newCategory.RemainingAmount < 0) newCategory.RemainingAmount = 0;
                        _unitOfWork.ExpenseCategories.Update(newCategory);
                    }

                    // Update expense fields
                    expense.Amount = model.Amount;
                    expense.Date = model.Date;
                    expense.Reason = model.Reason;
                    expense.CategoryId = model.CategoryId;
                    _unitOfWork.Expenses.Update(expense);

                    // Recalc new category for one-time budgets (expense now belongs to it)
                    if (newCategory.MonthlyFixedBudget == 0 && newCategory.BudgetAmount > 0)
                    {
                        newCategory.Expenses.Add(expense);
                        RecalcOneTimeBudget(newCategory);
                    }
                }
                else
                {
                    // Same category — update amount/date/reason
                    var amountDifference = model.Amount - oldAmount;

                    var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.CategoryId, userId);
                    if (category != null && category.BudgetAmount > 0 && amountDifference > 0)
                    {
                        // Check balance for increase
                        decimal available;
                        if (category.MonthlyFixedBudget > 0)
                        {
                            available = category.RemainingAmount;
                        }
                        else
                        {
                            available = CalcOneTimeRemaining(category);
                        }

                        if (available < amountDifference)
                        {
                            _logger.LogWarning("Insufficient balance for expense increase in category {CategoryId}", model.CategoryId);
                            return false;
                        }
                    }

                    expense.Amount = model.Amount;
                    expense.Date = model.Date;
                    expense.Reason = model.Reason;
                    _unitOfWork.Expenses.Update(expense);

                    if (category != null && amountDifference != 0)
                    {
                        if (category.MonthlyFixedBudget > 0)
                        {
                            category.RemainingAmount -= amountDifference;
                            if (category.RemainingAmount < 0) category.RemainingAmount = 0;
                            _unitOfWork.ExpenseCategories.Update(category);
                        }
                        else if (category.BudgetAmount > 0)
                        {
                            RecalcOneTimeBudget(category);
                        }
                    }
                }

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
        /// For BOTH monthly and one-time budgets: updates stored RemainingAmount.
        /// </summary>
        public async Task<bool> DeleteExpenseAsync(int id, string userId)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(id, userId);
                if (expense == null) return false;

                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(expense.CategoryId, userId);

                // Soft delete the expense first
                expense.IsDeleted = true;
                expense.DeletedAt = DateTime.UtcNow;
                _unitOfWork.Expenses.Update(expense);

                if (category != null)
                {
                    if (expense.Amount < 0 && category.MonthlyFixedBudget == 0)
                    {
                        // Deleting a funds-added entry: undo the BudgetAmount increase
                        var fundsAmount = -expense.Amount;
                        category.BudgetAmount -= fundsAmount;
                        if (category.BudgetAmount < 0) category.BudgetAmount = 0;
                        RecalcOneTimeBudget(category);
                    }
                    else if (category.MonthlyFixedBudget > 0)
                    {
                        // Monthly budget: refund
                        category.RemainingAmount += expense.Amount;
                        _unitOfWork.ExpenseCategories.Update(category);
                    }
                    else if (category.BudgetAmount > 0)
                    {
                        // One-time budget with positive expense: recalculate
                        // IsDeleted is already set, so recalc will exclude it
                        RecalcOneTimeBudget(category);
                    }
                }

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
        /// </summary>
        public async Task<List<ExpenseCategoryViewModel>> GetCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
            var allExpenses = (await _unitOfWork.Expenses.GetUserExpensesAsync(userId)).ToList();
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            
            return categories.Select(c => 
            {
                decimal remaining;
                decimal effectiveBudget;
                
                if (c.MonthlyFixedBudget > 0)
                {
                    decimal rollover = 0;
                    if (c.RolloverEnabled)
                    {
                        var prevMonthStart = startOfMonth.AddMonths(-1);
                        if (prevMonthStart >= BudgetStartDate)
                        {
                            var prevMonthEnd = prevMonthStart.AddMonths(1).AddDays(-1);
                            var prevSpent = allExpenses
                                .Where(e => e.CategoryId == c.Id && e.Amount > 0 && e.Date >= prevMonthStart && e.Date <= prevMonthEnd)
                                .Sum(e => e.Amount);
                            var prevLeftover = c.MonthlyFixedBudget - prevSpent;
                            if (prevLeftover > 0)
                                rollover = prevLeftover;
                        }
                    }

                    effectiveBudget = c.MonthlyFixedBudget + rollover;
                    var monthExpenses = allExpenses
                        .Where(e => e.CategoryId == c.Id && e.Amount > 0 && e.Date >= startOfMonth && e.Date < endOfMonth)
                        .Sum(e => e.Amount);
                    remaining = effectiveBudget - monthExpenses;
                }
                else if (c.BudgetAmount > 0)
                {
                    effectiveBudget = c.BudgetAmount;
                    var totalExpenses = allExpenses
                        .Where(e => e.CategoryId == c.Id && e.Amount > 0)
                        .Sum(e => e.Amount);
                    remaining = effectiveBudget - totalExpenses;
                }
                else
                {
                    effectiveBudget = 0;
                    remaining = 0;
                }
                
                // Only count actual positive expenses for the count
                var expenseCount = allExpenses.Count(e => e.CategoryId == c.Id && e.Amount > 0);
                
                return new ExpenseCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsDefault = c.IsDefault,
                    ExpenseCount = expenseCount,
                    BudgetAmount = effectiveBudget,
                    RemainingAmount = remaining > 0 ? remaining : 0,
                    MonthlyFixedBudget = c.MonthlyFixedBudget,
                    IsBudgetActive = c.IsBudgetActive,
                    IsHidden = c.IsHidden
                };
            }).ToList();
        }

        /// <summary>
        /// Gets a category for editing with dynamically calculated balance info
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
                decimal rollover = 0;
                if (category.RolloverEnabled)
                {
                    var prevMonthStart = startOfMonth.AddMonths(-1);
                    if (prevMonthStart >= BudgetStartDate)
                    {
                        var prevMonthEnd = prevMonthStart.AddMonths(1).AddDays(-1);
                        var prevSpent = category.Expenses
                            .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= prevMonthStart && e.Date <= prevMonthEnd)
                            .Sum(e => e.Amount);
                        var prevLeftover = category.MonthlyFixedBudget - prevSpent;
                        if (prevLeftover > 0)
                            rollover = prevLeftover;
                    }
                }

                budget = category.MonthlyFixedBudget + rollover;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= startOfMonth && e.Date < endOfMonth)
                    .Sum(e => e.Amount);
            }
            else
            {
                budget = category.BudgetAmount;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted && e.Amount > 0)
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
                MonthlyFixedBudget = category.MonthlyFixedBudget,
                IsBudgetActive = category.IsBudgetActive,
                RolloverEnabled = category.RolloverEnabled
            };
        }

        /// <summary>
        /// Gets category details for display/deletion with dynamically calculated balance
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
                decimal rollover = 0;
                if (category.RolloverEnabled)
                {
                    var prevMonthStart = startOfMonth.AddMonths(-1);
                    if (prevMonthStart >= BudgetStartDate)
                    {
                        var prevMonthEnd = prevMonthStart.AddMonths(1).AddDays(-1);
                        var prevSpent = category.Expenses
                            .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= prevMonthStart && e.Date <= prevMonthEnd)
                            .Sum(e => e.Amount);
                        var prevLeftover = category.MonthlyFixedBudget - prevSpent;
                        if (prevLeftover > 0)
                            rollover = prevLeftover;
                    }
                }

                budget = category.MonthlyFixedBudget + rollover;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= startOfMonth && e.Date < endOfMonth)
                    .Sum(e => e.Amount);
            }
            else
            {
                budget = category.BudgetAmount;
                totalExpenses = category.Expenses
                    .Where(e => !e.IsDeleted && e.Amount > 0)
                    .Sum(e => e.Amount);
            }
            
            var remaining = budget - totalExpenses;

            return new ExpenseCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault,
                ExpenseCount = category.Expenses.Count(e => !e.IsDeleted && e.Amount > 0),
                BudgetAmount = budget,
                RemainingAmount = remaining > 0 ? remaining : 0,
                MonthlyFixedBudget = category.MonthlyFixedBudget,
                IsBudgetActive = category.IsBudgetActive,
                IsHidden = category.IsHidden
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
                    RemainingAmount = budgetAmount,
                    MonthlyFixedBudget = model.MonthlyFixedBudget,
                    IsBudgetActive = model.IsBudgetActive,
                    RolloverEnabled = model.RolloverEnabled,
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
                category.IsBudgetActive = model.IsBudgetActive;
                category.RolloverEnabled = model.RolloverEnabled;

                // Determine budget amount
                var newBudget = model.MonthlyFixedBudget > 0 ? model.MonthlyFixedBudget : model.BudgetAmount;
                category.BudgetAmount = newBudget;

                // FIX: Calculate remaining based on budget type (exclude funds-added entries)
                if (newBudget > 0)
                {
                    decimal totalExpenses;
                    
                    if (model.MonthlyFixedBudget > 0)
                    {
                        // Monthly fixed budget: calculate from current month expenses only
                        var startOfMonth = new DateTime(now.Year, now.Month, 1);
                        totalExpenses = category.Expenses
                            .Where(e => !e.IsDeleted && e.Amount > 0 && e.Date >= startOfMonth && e.Date < startOfMonth.AddMonths(1))
                            .Sum(e => e.Amount);
                    }
                    else
                    {
                        // One-time/manual budget: calculate from ALL actual expenses (exclude funds-added)
                        totalExpenses = category.Expenses
                            .Where(e => !e.IsDeleted && e.Amount > 0)
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
        /// Soft deletes an expense category
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

                var totalExpenses = category.Expenses.Where(e => !e.IsDeleted && e.Amount > 0).Sum(e => e.Amount);

                category.BudgetAmount = budgetAmount;
                category.RemainingAmount = budgetAmount - totalExpenses;
                if (category.RemainingAmount < 0)
                    category.RemainingAmount = 0;

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

        /// <summary>
        /// Gets per-category monthly savings breakdown for all past months (monthly budget categories only)
        /// </summary>
        public async Task<MonthlySavingsViewModel> GetMonthlySavingsAsync(string userId)
        {
            var categories = await _unitOfWork.ExpenseCategories.GetUserCategoriesAsync(userId);
            var monthlyCats = categories.Where(c => c.MonthlyFixedBudget > 0).ToList();
            var allExpenses = (await _unitOfWork.Expenses.GetUserExpensesAsync(userId))
                .Where(e => e.Amount > 0).ToList();

            var today = DateTime.Today;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1);

            var monthlyBreakdown = new List<MonthSavingsRow>();
            var categoryTotals = monthlyCats.ToDictionary(c => c.Id, c => new CategorySavingsViewModel
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                MonthlyFixedBudget = c.MonthlyFixedBudget
            });

            var date = BudgetStartDate;
            while (date < currentMonthStart)
            {
                var monthStart = new DateTime(date.Year, date.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var row = new MonthSavingsRow
                {
                    Year = date.Year,
                    Month = date.Month,
                    MonthDisplay = monthStart.ToString("MMMM yyyy")
                };

                foreach (var cat in monthlyCats)
                {
                    var spent = allExpenses
                        .Where(e => e.CategoryId == cat.Id && e.Date >= monthStart && e.Date <= monthEnd)
                        .Sum(e => e.Amount);

                    var saved = Math.Max(cat.MonthlyFixedBudget - spent, 0);

                    row.CategorySavings.Add(new CategoryMonthSaving
                    {
                        CategoryId = cat.Id,
                        CategoryName = cat.Name,
                        Budget = cat.MonthlyFixedBudget,
                        Spent = spent,
                        Saved = saved
                    });

                    row.TotalBudget += cat.MonthlyFixedBudget;
                    row.TotalSpent += spent;
                    row.TotalSaved += saved;

                    categoryTotals[cat.Id].TotalSpent += spent;
                    categoryTotals[cat.Id].TotalSaved += saved;
                    categoryTotals[cat.Id].TotalBudgeted += cat.MonthlyFixedBudget;
                    categoryTotals[cat.Id].MonthCount++;
                }

                monthlyBreakdown.Add(row);
                date = date.AddMonths(1);
            }

            monthlyBreakdown.Reverse();

            return new MonthlySavingsViewModel
            {
                GrandTotalSavings = categoryTotals.Values.Sum(c => c.TotalSaved),
                TotalMonthlyFixedBudget = monthlyCats.Sum(c => c.MonthlyFixedBudget),
                Categories = categoryTotals.Values.OrderByDescending(c => c.TotalSaved).ToList(),
                MonthlyBreakdown = monthlyBreakdown
            };
        }

        /// <summary>
        /// Adds funds back to a one-time budget category.
        /// Increases BudgetAmount and recalculates RemainingAmount.
        /// Also records a negative expense as an audit trail.
        /// </summary>
        public async Task<bool> AddFundsToCategoryAsync(AddFundsViewModel model, string userId)
        {
            try
            {
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(model.CategoryId, userId);
                if (category == null)
                {
                    _logger.LogWarning("Category {CategoryId} not found for adding funds", model.CategoryId);
                    return false;
                }

                // Only allow adding funds to one-time budget categories (not monthly)
                if (category.MonthlyFixedBudget > 0)
                {
                    _logger.LogWarning("Cannot add funds to monthly budget category {CategoryId}", model.CategoryId);
                    return false;
                }

                // Increase BudgetAmount
                category.BudgetAmount += model.Amount;

                // Recalculate RemainingAmount from scratch
                var totalSpent = category.Expenses
                    .Where(e => !e.IsDeleted && e.Amount > 0)
                    .Sum(e => e.Amount);
                category.RemainingAmount = category.BudgetAmount - totalSpent;
                if (category.RemainingAmount < 0)
                    category.RemainingAmount = 0;

                _unitOfWork.ExpenseCategories.Update(category);

                // Record a negative expense as audit trail
                var auditExpense = new Expense
                {
                    Amount = -model.Amount,
                    Date = model.Date,
                    Reason = $"[FUNDS ADDED] {model.Reason}",
                    CategoryId = model.CategoryId,
                    UserId = userId
                };
                await _unitOfWork.Expenses.AddAsync(auditExpense);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Funds added to category {CategoryName}: Amount={Amount}, NewBudget={Budget}, Remaining={Remaining}",
                    category.Name, model.Amount, category.BudgetAmount, category.RemainingAmount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding funds to category {CategoryId} for user {UserId}", model.CategoryId, userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing funds-added entry (legacy negative expense).
        /// Adjusts BudgetAmount to reflect the change in funds amount and recalculates.
        /// </summary>
        public async Task<bool> UpdateFundsEntryAsync(EditFundsViewModel model, string userId)
        {
            try
            {
                var expense = await _unitOfWork.Expenses.GetExpenseWithCategoryAsync(model.Id, userId);
                if (expense == null) return false;

                // Ensure this is actually a funds-added entry
                if (expense.Amount >= 0)
                {
                    _logger.LogWarning("Attempted to update non-funds entry {ExpenseId} via UpdateFundsEntryAsync", model.Id);
                    return false;
                }

                // Validate date is not in the future
                if (model.Date.Date > DateTime.Today)
                {
                    _logger.LogWarning("Attempted to update funds entry with future date");
                    return false;
                }

                // Calculate the difference in funds amount and adjust BudgetAmount
                var oldFundsAmount = -expense.Amount; // old positive amount
                var newFundsAmount = model.Amount;     // new positive amount
                var difference = newFundsAmount - oldFundsAmount;

                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(expense.CategoryId, userId);
                if (category != null && category.MonthlyFixedBudget == 0)
                {
                    // Adjust BudgetAmount by the difference
                    category.BudgetAmount += difference;
                    if (category.BudgetAmount < 0) category.BudgetAmount = 0;

                    // Recalculate remaining
                    var totalSpent = category.Expenses
                        .Where(e => !e.IsDeleted && e.Amount > 0)
                        .Sum(e => e.Amount);
                    category.RemainingAmount = category.BudgetAmount - totalSpent;
                    if (category.RemainingAmount < 0) category.RemainingAmount = 0;

                    _unitOfWork.ExpenseCategories.Update(category);
                }

                expense.Amount = -model.Amount;
                expense.Date = model.Date;
                expense.Reason = $"[FUNDS ADDED] {model.Reason}";

                _unitOfWork.Expenses.Update(expense);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Funds entry {ExpenseId} updated for user {UserId}, budget adjusted by {Difference}", model.Id, userId, difference);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating funds entry {ExpenseId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Toggles the IsHidden flag on a category (hide/unhide from dashboard and dropdowns)
        /// </summary>
        public async Task<bool> ToggleCategoryHiddenAsync(int categoryId, string userId)
        {
            try
            {
                var category = await _unitOfWork.ExpenseCategories.GetCategoryWithExpensesAsync(categoryId, userId);
                if (category == null) return false;

                category.IsHidden = !category.IsHidden;
                _unitOfWork.ExpenseCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category {CategoryId} IsHidden toggled to {IsHidden} for user {UserId}", categoryId, category.IsHidden, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling IsHidden for category {CategoryId} for user {UserId}", categoryId, userId);
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
