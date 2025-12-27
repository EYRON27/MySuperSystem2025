using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Expense category repository interface
    /// </summary>
    public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
    {
        Task<IEnumerable<ExpenseCategory>> GetUserCategoriesAsync(string userId);
        Task<ExpenseCategory?> GetCategoryWithExpensesAsync(int id, string userId);
        Task<bool> CategoryNameExistsAsync(string userId, string name, int? excludeId = null);
    }
}
