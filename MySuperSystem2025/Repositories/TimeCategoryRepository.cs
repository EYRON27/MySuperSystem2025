using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Repository implementation for TimeCategory entity.
    /// </summary>
    public class TimeCategoryRepository : Repository<TimeCategory>, ITimeCategoryRepository
    {
        public TimeCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimeCategory>> GetUserCategoriesAsync(string userId)
        {
            return await _context.TimeCategories
                .Include(c => c.TimeEntries)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<TimeCategory?> GetCategoryWithTimeEntriesAsync(int id, string userId)
        {
            return await _context.TimeCategories
                .Include(c => c.TimeEntries)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<bool> CategoryNameExistsAsync(string userId, string name, int? excludeId = null)
        {
            var query = _context.TimeCategories
                .Where(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
