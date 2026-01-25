using MySuperSystem2025.Models.ViewModels.Food;

namespace MySuperSystem2025.Services.Interfaces
{
    /// <summary>
    /// Service interface for food tracking operations.
    /// </summary>
    public interface IFoodService
    {
        // Dashboard
        Task<FoodDashboardViewModel> GetDashboardAsync(string userId, string? breakdownPeriod = null);

        // Food Entries
        Task<FoodEntryListViewModel> GetFoodEntriesAsync(string userId, string? period = null, string? mealType = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<EditFoodEntryViewModel?> GetFoodEntryForEditAsync(int id, string userId);
        Task<FoodEntryListItemViewModel?> GetFoodEntryDetailsAsync(int id, string userId);
        Task<bool> CreateFoodEntryAsync(CreateFoodEntryViewModel model, string userId);
        Task<bool> UpdateFoodEntryAsync(EditFoodEntryViewModel model, string userId);
        Task<bool> DeleteFoodEntryAsync(int id, string userId);

        // Statistics
        Task<int> GetTodayCaloriesAsync(string userId);
        Task<(decimal protein, decimal carbs, decimal fats)> GetTodayMacrosAsync(string userId);
    }
}
