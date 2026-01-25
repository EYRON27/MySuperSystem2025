using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Repository implementation for FoodEntry entity.
    /// </summary>
    public class FoodEntryRepository : Repository<FoodEntry>, IFoodEntryRepository
    {
        public FoodEntryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FoodEntry>> GetUserFoodEntriesAsync(string userId)
        {
            return await _context.FoodEntries
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.Date)
                .ThenBy(f => f.MealType)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodEntry>> GetUserFoodEntriesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.FoodEntries
                .Where(f => f.UserId == userId && f.Date.Date >= startDate.Date && f.Date.Date <= endDate.Date)
                .OrderByDescending(f => f.Date)
                .ThenBy(f => f.MealType)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodEntry>> GetUserFoodEntriesByDateAsync(string userId, DateTime date)
        {
            return await _context.FoodEntries
                .Where(f => f.UserId == userId && f.Date.Date == date.Date)
                .OrderBy(f => f.MealType)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodEntry>> GetUserFoodEntriesByMealTypeAsync(string userId, string mealType)
        {
            return await _context.FoodEntries
                .Where(f => f.UserId == userId && f.MealType == mealType)
                .OrderByDescending(f => f.Date)
                .ToListAsync();
        }

        public async Task<FoodEntry?> GetFoodEntryAsync(int id, string userId)
        {
            return await _context.FoodEntries
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        }
    }
}
