using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Expense repository implementation with specialized queries
    /// </summary>
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Expense>> GetUserExpensesAsync(string userId)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetUserExpensesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetUserExpensesByCategoryAsync(string userId, int categoryId)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.CategoryId == categoryId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .SumAsync(e => e.Amount);
        }

        public async Task<Expense?> GetExpenseWithCategoryAsync(int id, string userId)
        {
            return await _dbSet
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }
    }
}
