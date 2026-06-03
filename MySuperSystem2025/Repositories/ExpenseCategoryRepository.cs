using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Expense category repository implementation
    /// </summary>
    public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
    {
        public ExpenseCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExpenseCategory>> GetUserCategoriesAsync(string userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ExpenseCategory?> GetCategoryWithExpensesAsync(int id, string userId)
        {
            return await _dbSet
                .Include(c => c.Expenses.Where(e => !e.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        }

        public async Task<bool> CategoryNameExistsAsync(string userId, string name, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
