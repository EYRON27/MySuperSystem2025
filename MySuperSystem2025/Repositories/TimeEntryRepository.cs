using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Repository implementation for TimeEntry entity.
    /// </summary>
    public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
    {
        public TimeEntryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimeEntry>> GetUserTimeEntriesAsync(string userId)
        {
            return await _context.TimeEntries
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetUserTimeEntriesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.TimeEntries
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.StartTime.Date >= startDate.Date && t.StartTime.Date <= endDate.Date)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetUserTimeEntriesByCategoryAsync(string userId, int categoryId)
        {
            return await _context.TimeEntries
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.CategoryId == categoryId)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<TimeEntry?> GetTimeEntryWithCategoryAsync(int id, string userId)
        {
            return await _context.TimeEntries
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }
    }
}
