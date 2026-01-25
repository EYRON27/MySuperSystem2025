using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for FoodEntry operations.
    /// </summary>
    public interface IFoodEntryRepository : IRepository<FoodEntry>
    {
        Task<IEnumerable<FoodEntry>> GetUserFoodEntriesAsync(string userId);
        Task<IEnumerable<FoodEntry>> GetUserFoodEntriesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<FoodEntry>> GetUserFoodEntriesByDateAsync(string userId, DateTime date);
        Task<IEnumerable<FoodEntry>> GetUserFoodEntriesByMealTypeAsync(string userId, string mealType);
        Task<FoodEntry?> GetFoodEntryAsync(int id, string userId);
    }
}
