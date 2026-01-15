using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Time;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services.Interfaces;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Time service implementation handling all time tracking business logic.
    /// </summary>
    public class TimeService : ITimeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TimeService> _logger;

        public TimeService(IUnitOfWork unitOfWork, ILogger<TimeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data with time tracking summaries.
        /// </summary>
        public async Task<TimeDashboardViewModel> GetDashboardAsync(string userId, string? breakdownPeriod = null)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var allEntries = await _unitOfWork.TimeEntries.GetUserTimeEntriesAsync(userId);
            var entriesList = allEntries.ToList();

            var todayEntries = entriesList.Where(e => e.StartTime.Date == today).ToList();
            var weeklyEntries = entriesList.Where(e => e.StartTime.Date >= startOfWeek && e.StartTime.Date <= today).ToList();
            var monthlyEntries = entriesList.Where(e => e.StartTime.Date >= startOfMonth && e.StartTime.Date <= today).ToList();

            // Category breakdown based on selected period
            List<TimeEntry> breakdownEntries;
            string breakdownPeriodName;

            switch (breakdownPeriod?.ToLower())
            {
                case "daily":
                    breakdownEntries = todayEntries;
                    breakdownPeriodName = "Today";
                    break;
                case "weekly":
                    breakdownEntries = weeklyEntries;
                    breakdownPeriodName = "This Week";
                    break;
                case "monthly":
                default:
                    breakdownEntries = monthlyEntries;
                    breakdownPeriodName = "This Month";
                    break;
            }

            var categoryBreakdown = breakdownEntries
                .GroupBy(e => new { e.CategoryId, Name = e.Category?.Name ?? "Uncategorized" })
                .Select(g => new CategorySummaryViewModel
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalMinutes = g.Sum(e => e.DurationMinutes),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.TotalMinutes)
                .ToList();

            var breakdownTotal = breakdownEntries.Sum(e => e.DurationMinutes);
            foreach (var category in categoryBreakdown)
            {
                category.Percentage = breakdownTotal > 0
                    ? Math.Round((decimal)category.TotalMinutes / breakdownTotal * 100, 1)
                    : 0;
            }

            return new TimeDashboardViewModel
            {
                TodayMinutes = todayEntries.Sum(e => e.DurationMinutes),
                WeeklyMinutes = weeklyEntries.Sum(e => e.DurationMinutes),
                MonthlyMinutes = monthlyEntries.Sum(e => e.DurationMinutes),
                TodayCount = todayEntries.Count,
                WeeklyCount = weeklyEntries.Count,
                MonthlyCount = monthlyEntries.Count,
                RecentEntries = entriesList.Take(10).Select(MapToListItem).ToList(),
                CategoryBreakdown = categoryBreakdown,
                BreakdownPeriod = breakdownPeriod ?? "monthly",
                BreakdownPeriodName = breakdownPeriodName
            };
        }

        /// <summary>
        /// Gets filtered list of time entries.
        /// </summary>
        public async Task<TimeEntryListViewModel> GetTimeEntriesAsync(string userId, string? period = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            IEnumerable<TimeEntry> entries;
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
                entries = await _unitOfWork.TimeEntries.GetUserTimeEntriesByDateRangeAsync(userId, startDate.Value, endDate.Value);
            }
            else if (categoryId.HasValue)
            {
                entries = await _unitOfWork.TimeEntries.GetUserTimeEntriesByCategoryAsync(userId, categoryId.Value);
            }
            else
            {
                entries = await _unitOfWork.TimeEntries.GetUserTimeEntriesAsync(userId);
            }

            // Apply category filter if specified
            if (categoryId.HasValue)
            {
                entries = entries.Where(e => e.CategoryId == categoryId.Value);
            }

            var entriesList = entries.ToList();
            var categories = await GetCategoriesAsync(userId);

            return new TimeEntryListViewModel
            {
                TimeEntries = entriesList.Select(MapToListItem).ToList(),
                FilterPeriod = period,
                FilterCategoryId = categoryId,
                FilterStartDate = startDate,
                FilterEndDate = endDate,
                TotalMinutes = entriesList.Sum(e => e.DurationMinutes),
                TotalCount = entriesList.Count,
                Categories = categories
            };
        }

        /// <summary>
        /// Gets a time entry for editing.
        /// </summary>
        public async Task<EditTimeEntryViewModel?> GetTimeEntryForEditAsync(int id, string userId)
        {
            var entry = await _unitOfWork.TimeEntries.GetTimeEntryWithCategoryAsync(id, userId);
            if (entry == null) return null;

            return new EditTimeEntryViewModel
            {
                Id = entry.Id,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                Notes = entry.Notes,
                CategoryId = entry.CategoryId
            };
        }

        /// <summary>
        /// Gets time entry details for display/deletion.
        /// </summary>
        public async Task<TimeEntryListItemViewModel?> GetTimeEntryDetailsAsync(int id, string userId)
        {
            var entry = await _unitOfWork.TimeEntries.GetTimeEntryWithCategoryAsync(id, userId);
            if (entry == null) return null;

            return MapToListItem(entry);
        }

        /// <summary>
        /// Creates a new time entry.
        /// </summary>
        public async Task<bool> CreateTimeEntryAsync(CreateTimeEntryViewModel model, string userId)
        {
            try
            {
                // Validate end time is after start time
                if (model.EndTime <= model.StartTime)
                {
                    _logger.LogWarning("End time must be after start time");
                    return false;
                }

                // Validate times are not in the future
                if (model.EndTime > DateTime.Now)
                {
                    _logger.LogWarning("End time cannot be in the future");
                    return false;
                }

                // Calculate duration
                var duration = (int)(model.EndTime - model.StartTime).TotalMinutes;

                var entry = new TimeEntry
                {
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    DurationMinutes = duration,
                    Notes = model.Notes,
                    CategoryId = model.CategoryId,
                    UserId = userId
                };

                await _unitOfWork.TimeEntries.AddAsync(entry);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Time entry created successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating time entry for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing time entry.
        /// </summary>
        public async Task<bool> UpdateTimeEntryAsync(EditTimeEntryViewModel model, string userId)
        {
            try
            {
                var entry = await _unitOfWork.TimeEntries.GetTimeEntryWithCategoryAsync(model.Id, userId);
                if (entry == null) return false;

                // Validate end time is after start time
                if (model.EndTime <= model.StartTime)
                {
                    _logger.LogWarning("End time must be after start time");
                    return false;
                }

                // Validate times are not in the future
                if (model.EndTime > DateTime.Now)
                {
                    _logger.LogWarning("End time cannot be in the future");
                    return false;
                }

                // Calculate duration
                var duration = (int)(model.EndTime - model.StartTime).TotalMinutes;

                entry.StartTime = model.StartTime;
                entry.EndTime = model.EndTime;
                entry.DurationMinutes = duration;
                entry.Notes = model.Notes;
                entry.CategoryId = model.CategoryId;

                _unitOfWork.TimeEntries.Update(entry);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Time entry {EntryId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating time entry {EntryId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a time entry.
        /// </summary>
        public async Task<bool> DeleteTimeEntryAsync(int id, string userId)
        {
            try
            {
                var entry = await _unitOfWork.TimeEntries.GetTimeEntryWithCategoryAsync(id, userId);
                if (entry == null) return false;

                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.UtcNow;

                _unitOfWork.TimeEntries.Update(entry);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Time entry {EntryId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting time entry {EntryId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Gets all categories for a user.
        /// </summary>
        public async Task<List<TimeCategoryViewModel>> GetCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.TimeCategories.GetUserCategoriesAsync(userId);
            return categories.Select(c => new TimeCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsDefault = c.IsDefault,
                EntryCount = c.TimeEntries.Count
            }).ToList();
        }

        /// <summary>
        /// Gets a category for editing.
        /// </summary>
        public async Task<EditTimeCategoryViewModel?> GetCategoryForEditAsync(int id, string userId)
        {
            var category = await _unitOfWork.TimeCategories.GetCategoryWithTimeEntriesAsync(id, userId);
            if (category == null) return null;

            return new EditTimeCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault,
                TotalEntries = category.TimeEntries.Count
            };
        }

        /// <summary>
        /// Gets category details for display/deletion.
        /// </summary>
        public async Task<TimeCategoryViewModel?> GetCategoryDetailsAsync(int id, string userId)
        {
            var category = await _unitOfWork.TimeCategories.GetCategoryWithTimeEntriesAsync(id, userId);
            if (category == null) return null;

            return new TimeCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault,
                EntryCount = category.TimeEntries.Count
            };
        }

        /// <summary>
        /// Creates a new time category.
        /// </summary>
        public async Task<bool> CreateCategoryAsync(CreateTimeCategoryViewModel model, string userId)
        {
            try
            {
                // Check if category name already exists
                if (await _unitOfWork.TimeCategories.CategoryNameExistsAsync(userId, model.Name))
                {
                    _logger.LogWarning("Category name already exists for user {UserId}", userId);
                    return false;
                }

                var category = new TimeCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    UserId = userId,
                    IsDefault = false
                };

                await _unitOfWork.TimeCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Time category created for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating time category for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates a time category.
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(EditTimeCategoryViewModel model, string userId)
        {
            try
            {
                var category = await _unitOfWork.TimeCategories.GetCategoryWithTimeEntriesAsync(model.Id, userId);
                if (category == null) return false;

                // Check if category name already exists (excluding current)
                if (await _unitOfWork.TimeCategories.CategoryNameExistsAsync(userId, model.Name, model.Id))
                {
                    _logger.LogWarning("Category name already exists for user {UserId}", userId);
                    return false;
                }

                category.Name = model.Name;
                category.Description = model.Description;

                _unitOfWork.TimeCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Time category {CategoryId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating time category {CategoryId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a time category.
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            try
            {
                var category = await _unitOfWork.TimeCategories.GetCategoryWithTimeEntriesAsync(id, userId);
                if (category == null) return false;

                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;

                _unitOfWork.TimeCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Time category {CategoryId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting time category {CategoryId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Seeds default time categories for a new user.
        /// </summary>
        public async Task SeedDefaultCategoriesAsync(string userId)
        {
            var defaultCategories = new[]
            {
                new TimeCategory { Name = "Work", Description = "Work-related activities", UserId = userId, IsDefault = true },
                new TimeCategory { Name = "Study", Description = "Learning and education", UserId = userId, IsDefault = true },
                new TimeCategory { Name = "Games", Description = "Gaming and entertainment", UserId = userId, IsDefault = true },
                new TimeCategory { Name = "Exercise", Description = "Physical activities", UserId = userId, IsDefault = true }
            };

            foreach (var category in defaultCategories)
            {
                if (!await _unitOfWork.TimeCategories.CategoryNameExistsAsync(userId, category.Name))
                {
                    await _unitOfWork.TimeCategories.AddAsync(category);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Default time categories seeded for user {UserId}", userId);
        }

        private static TimeEntryListItemViewModel MapToListItem(TimeEntry entry)
        {
            return new TimeEntryListItemViewModel
            {
                Id = entry.Id,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                DurationMinutes = entry.DurationMinutes,
                Notes = entry.Notes,
                CategoryName = entry.Category?.Name ?? "Uncategorized",
                CategoryId = entry.CategoryId
            };
        }
    }
}
