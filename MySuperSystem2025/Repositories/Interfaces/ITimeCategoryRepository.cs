using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for TimeCategory operations.
    /// </summary>
    public interface ITimeCategoryRepository : IRepository<TimeCategory>
    {
        Task<IEnumerable<TimeCategory>> GetUserCategoriesAsync(string userId);
        Task<TimeCategory?> GetCategoryWithTimeEntriesAsync(int id, string userId);
        Task<bool> CategoryNameExistsAsync(string userId, string name, int? excludeId = null);
    }
}
