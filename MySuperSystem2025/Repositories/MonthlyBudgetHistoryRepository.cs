using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    public class MonthlyBudgetHistoryRepository : Repository<MonthlyBudgetHistory>, IMonthlyBudgetHistoryRepository
    {
        public MonthlyBudgetHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MonthlyBudgetHistory?> GetHistoryAsync(string userId, int categoryId, int year, int month)
        {
            return await _dbSet
                .FirstOrDefaultAsync(h => h.UserId == userId && h.CategoryId == categoryId && h.Year == year && h.Month == month);
        }

        public async Task<List<MonthlyBudgetHistory>> GetHistoriesForMonthAsync(string userId, int year, int month)
        {
            return await _dbSet
                .Where(h => h.UserId == userId && h.Year == year && h.Month == month)
                .ToListAsync();
        }
    }
}
