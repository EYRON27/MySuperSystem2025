using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for TimeEntry operations.
    /// </summary>
    public interface ITimeEntryRepository : IRepository<TimeEntry>
    {
        Task<IEnumerable<TimeEntry>> GetUserTimeEntriesAsync(string userId);
        Task<IEnumerable<TimeEntry>> GetUserTimeEntriesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<TimeEntry>> GetUserTimeEntriesByCategoryAsync(string userId, int categoryId);
        Task<TimeEntry?> GetTimeEntryWithCategoryAsync(int id, string userId);
    }
}
