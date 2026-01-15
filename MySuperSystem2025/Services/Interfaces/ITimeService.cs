using MySuperSystem2025.Models.ViewModels.Time;

namespace MySuperSystem2025.Services.Interfaces
{
    /// <summary>
    /// Service interface for time tracking operations.
    /// </summary>
    public interface ITimeService
    {
        // Dashboard
        Task<TimeDashboardViewModel> GetDashboardAsync(string userId, string? breakdownPeriod = null);

        // Time Entries
        Task<TimeEntryListViewModel> GetTimeEntriesAsync(string userId, string? period = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<EditTimeEntryViewModel?> GetTimeEntryForEditAsync(int id, string userId);
        Task<TimeEntryListItemViewModel?> GetTimeEntryDetailsAsync(int id, string userId);
        Task<bool> CreateTimeEntryAsync(CreateTimeEntryViewModel model, string userId);
        Task<bool> UpdateTimeEntryAsync(EditTimeEntryViewModel model, string userId);
        Task<bool> DeleteTimeEntryAsync(int id, string userId);

        // Categories
        Task<List<TimeCategoryViewModel>> GetCategoriesAsync(string userId);
        Task<EditTimeCategoryViewModel?> GetCategoryForEditAsync(int id, string userId);
        Task<TimeCategoryViewModel?> GetCategoryDetailsAsync(int id, string userId);
        Task<bool> CreateCategoryAsync(CreateTimeCategoryViewModel model, string userId);
        Task<bool> UpdateCategoryAsync(EditTimeCategoryViewModel model, string userId);
        Task<bool> DeleteCategoryAsync(int id, string userId);
        Task SeedDefaultCategoriesAsync(string userId);
    }
}
