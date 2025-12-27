using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Expense repository interface with specialized expense queries
    /// </summary>
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetUserExpensesAsync(string userId);
        Task<IEnumerable<Expense>> GetUserExpensesByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Expense>> GetUserExpensesByCategoryAsync(string userId, int categoryId);
        Task<decimal> GetTotalAmountAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Expense?> GetExpenseWithCategoryAsync(int id, string userId);
    }
}
