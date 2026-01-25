using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Food;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services.Interfaces;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Food service implementation handling all food tracking business logic.
    /// </summary>
    public class FoodService : IFoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FoodService> _logger;

        public FoodService(IUnitOfWork unitOfWork, ILogger<FoodService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data with food tracking summaries.
        /// </summary>
        public async Task<FoodDashboardViewModel> GetDashboardAsync(string userId, string? breakdownPeriod = null)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var allEntries = await _unitOfWork.FoodEntries.GetUserFoodEntriesAsync(userId);
            var entriesList = allEntries.ToList();

            var todayEntries = entriesList.Where(e => e.Date.Date == today).ToList();
            var weeklyEntries = entriesList.Where(e => e.Date.Date >= startOfWeek && e.Date.Date <= today).ToList();
            var monthlyEntries = entriesList.Where(e => e.Date.Date >= startOfMonth && e.Date.Date <= today).ToList();

            // Meal type breakdown based on selected period
            List<FoodEntry> breakdownEntries;
            string breakdownPeriodName;
            int daysInPeriod;

            switch (breakdownPeriod?.ToLower())
            {
                case "weekly":
                    breakdownEntries = weeklyEntries;
                    breakdownPeriodName = "This Week";
                    daysInPeriod = (int)(today - startOfWeek).TotalDays + 1;
                    break;
                case "monthly":
                    breakdownEntries = monthlyEntries;
                    breakdownPeriodName = "This Month";
                    daysInPeriod = (int)(today - startOfMonth).TotalDays + 1;
                    break;
                case "daily":
                default:
                    breakdownEntries = todayEntries;
                    breakdownPeriodName = "Today";
                    daysInPeriod = 1;
                    break;
            }

            var mealTypeBreakdown = breakdownEntries
                .GroupBy(e => e.MealType)
                .Select(g => new MealTypeSummaryViewModel
                {
                    MealType = g.Key,
                    Count = g.Count(),
                    TotalCalories = g.Sum(e => e.Calories),
                    TotalProtein = g.Sum(e => e.Protein),
                    TotalCarbs = g.Sum(e => e.Carbs),
                    TotalFats = g.Sum(e => e.Fats)
                })
                .OrderBy(m => GetMealOrder(m.MealType))
                .ToList();

            var breakdownTotalCalories = breakdownEntries.Sum(e => e.Calories);
            foreach (var mealType in mealTypeBreakdown)
            {
                mealType.Percentage = breakdownTotalCalories > 0
                    ? Math.Round((decimal)mealType.TotalCalories / breakdownTotalCalories * 100, 1)
                    : 0;
            }

            // Calculate averages
            var avgCalories = daysInPeriod > 0 ? breakdownEntries.Sum(e => e.Calories) / daysInPeriod : 0;
            var avgProtein = daysInPeriod > 0 ? breakdownEntries.Sum(e => e.Protein) / daysInPeriod : 0;
            var avgCarbs = daysInPeriod > 0 ? breakdownEntries.Sum(e => e.Carbs) / daysInPeriod : 0;
            var avgFats = daysInPeriod > 0 ? breakdownEntries.Sum(e => e.Fats) / daysInPeriod : 0;

            return new FoodDashboardViewModel
            {
                // Today
                TodayCalories = todayEntries.Sum(e => e.Calories),
                TodayProtein = todayEntries.Sum(e => e.Protein),
                TodayCarbs = todayEntries.Sum(e => e.Carbs),
                TodayFats = todayEntries.Sum(e => e.Fats),
                TodayMealCount = todayEntries.Count,

                // Weekly
                WeeklyCalories = weeklyEntries.Sum(e => e.Calories),
                WeeklyProtein = weeklyEntries.Sum(e => e.Protein),
                WeeklyCarbs = weeklyEntries.Sum(e => e.Carbs),
                WeeklyFats = weeklyEntries.Sum(e => e.Fats),
                WeeklyMealCount = weeklyEntries.Count,

                // Monthly
                MonthlyCalories = monthlyEntries.Sum(e => e.Calories),
                MonthlyProtein = monthlyEntries.Sum(e => e.Protein),
                MonthlyCarbs = monthlyEntries.Sum(e => e.Carbs),
                MonthlyFats = monthlyEntries.Sum(e => e.Fats),
                MonthlyMealCount = monthlyEntries.Count,

                // Breakdown
                MealTypeBreakdown = mealTypeBreakdown,
                BreakdownPeriod = breakdownPeriod ?? "daily",
                BreakdownPeriodName = breakdownPeriodName,

                // Recent entries
                RecentEntries = entriesList.Take(10).Select(MapToListItem).ToList(),

                // Averages
                AverageDailyCalories = avgCalories,
                AverageDailyProtein = avgProtein,
                AverageDailyCarbs = avgCarbs,
                AverageDailyFats = avgFats
            };
        }

        /// <summary>
        /// Gets filtered list of food entries.
        /// </summary>
        public async Task<FoodEntryListViewModel> GetFoodEntriesAsync(string userId, string? period = null, string? mealType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            IEnumerable<FoodEntry> entries;
            var today = DateTime.Today;

            // Determine date range based on period
            if (!string.IsNullOrEmpty(period))
            {
                (startDate, endDate) = period.ToLower() switch
                {
                    "daily" => (today, today),
                    "weekly" => (today.AddDays(-(int)today.DayOfWeek), today),
                    "monthly" => (new DateTime(today.Year, today.Month, 1), today),
                    _ => (startDate, endDate)
                };
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                entries = await _unitOfWork.FoodEntries.GetUserFoodEntriesByDateRangeAsync(userId, startDate.Value, endDate.Value);
            }
            else if (!string.IsNullOrEmpty(mealType))
            {
                entries = await _unitOfWork.FoodEntries.GetUserFoodEntriesByMealTypeAsync(userId, mealType);
            }
            else
            {
                entries = await _unitOfWork.FoodEntries.GetUserFoodEntriesAsync(userId);
            }

            // Apply meal type filter if specified
            if (!string.IsNullOrEmpty(mealType))
            {
                entries = entries.Where(e => e.MealType == mealType);
            }

            var entriesList = entries.ToList();

            return new FoodEntryListViewModel
            {
                FoodEntries = entriesList.Select(MapToListItem).ToList(),
                FilterPeriod = period,
                FilterMealType = mealType,
                FilterStartDate = startDate,
                FilterEndDate = endDate,
                TotalCount = entriesList.Count,
                TotalCalories = entriesList.Sum(e => e.Calories),
                TotalProtein = entriesList.Sum(e => e.Protein),
                TotalCarbs = entriesList.Sum(e => e.Carbs),
                TotalFats = entriesList.Sum(e => e.Fats)
            };
        }

        /// <summary>
        /// Gets a food entry for editing.
        /// </summary>
        public async Task<EditFoodEntryViewModel?> GetFoodEntryForEditAsync(int id, string userId)
        {
            var entry = await _unitOfWork.FoodEntries.GetFoodEntryAsync(id, userId);
            if (entry == null) return null;

            return new EditFoodEntryViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                MealType = entry.MealType,
                Date = entry.Date,
                ServingSize = entry.ServingSize,
                Calories = entry.Calories,
                Protein = entry.Protein,
                Carbs = entry.Carbs,
                Fats = entry.Fats,
                Notes = entry.Notes
            };
        }

        /// <summary>
        /// Gets food entry details for display/deletion.
        /// </summary>
        public async Task<FoodEntryListItemViewModel?> GetFoodEntryDetailsAsync(int id, string userId)
        {
            var entry = await _unitOfWork.FoodEntries.GetFoodEntryAsync(id, userId);
            if (entry == null) return null;

            return MapToListItem(entry);
        }

        /// <summary>
        /// Creates a new food entry.
        /// </summary>
        public async Task<bool> CreateFoodEntryAsync(CreateFoodEntryViewModel model, string userId)
        {
            try
            {
                // Validate date is not in the future
                if (model.Date.Date > DateTime.Today)
                {
                    _logger.LogWarning("Date cannot be in the future");
                    return false;
                }

                var entry = new FoodEntry
                {
                    Name = model.Name,
                    MealType = model.MealType,
                    Date = model.Date,
                    ServingSize = model.ServingSize,
                    Calories = model.Calories,
                    Protein = model.Protein,
                    Carbs = model.Carbs,
                    Fats = model.Fats,
                    Notes = model.Notes,
                    UserId = userId
                };

                await _unitOfWork.FoodEntries.AddAsync(entry);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Food entry created successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating food entry for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing food entry.
        /// </summary>
        public async Task<bool> UpdateFoodEntryAsync(EditFoodEntryViewModel model, string userId)
        {
            try
            {
                var entry = await _unitOfWork.FoodEntries.GetFoodEntryAsync(model.Id, userId);
                if (entry == null) return false;

                // Validate date is not in the future
                if (model.Date.Date > DateTime.Today)
                {
                    _logger.LogWarning("Date cannot be in the future");
                    return false;
                }

                entry.Name = model.Name;
                entry.MealType = model.MealType;
                entry.Date = model.Date;
                entry.ServingSize = model.ServingSize;
                entry.Calories = model.Calories;
                entry.Protein = model.Protein;
                entry.Carbs = model.Carbs;
                entry.Fats = model.Fats;
                entry.Notes = model.Notes;

                _unitOfWork.FoodEntries.Update(entry);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Food entry {EntryId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating food entry {EntryId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a food entry.
        /// </summary>
        public async Task<bool> DeleteFoodEntryAsync(int id, string userId)
        {
            try
            {
                var entry = await _unitOfWork.FoodEntries.GetFoodEntryAsync(id, userId);
                if (entry == null) return false;

                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.UtcNow;

                _unitOfWork.FoodEntries.Update(entry);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Food entry {EntryId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting food entry {EntryId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Gets today's total calories for a user.
        /// </summary>
        public async Task<int> GetTodayCaloriesAsync(string userId)
        {
            var todayEntries = await _unitOfWork.FoodEntries.GetUserFoodEntriesByDateAsync(userId, DateTime.Today);
            return todayEntries.Sum(e => e.Calories);
        }

        /// <summary>
        /// Gets today's macros for a user.
        /// </summary>
        public async Task<(decimal protein, decimal carbs, decimal fats)> GetTodayMacrosAsync(string userId)
        {
            var todayEntries = await _unitOfWork.FoodEntries.GetUserFoodEntriesByDateAsync(userId, DateTime.Today);
            var entriesList = todayEntries.ToList();
            return (
                entriesList.Sum(e => e.Protein),
                entriesList.Sum(e => e.Carbs),
                entriesList.Sum(e => e.Fats)
            );
        }

        private static FoodEntryListItemViewModel MapToListItem(FoodEntry entry)
        {
            return new FoodEntryListItemViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                MealType = entry.MealType,
                Date = entry.Date,
                ServingSize = entry.ServingSize,
                Calories = entry.Calories,
                Protein = entry.Protein,
                Carbs = entry.Carbs,
                Fats = entry.Fats,
                Notes = entry.Notes
            };
        }

        private static int GetMealOrder(string mealType)
        {
            return mealType switch
            {
                MealTypes.Breakfast => 1,
                MealTypes.Lunch => 2,
                MealTypes.Dinner => 3,
                MealTypes.Snack => 4,
                _ => 5
            };
        }
    }
}
