using MySuperSystem2025.Models.ViewModels.Expense;

namespace MySuperSystem2025.Services.Interfaces
{
    /// <summary>
    /// Expense service interface for business logic operations with balance tracking
    /// </summary>
    public interface IExpenseService
    {
        Task<ExpenseDashboardViewModel> GetDashboardAsync(string userId, string? breakdownPeriod = null);
        Task<ExpenseListViewModel> GetExpensesAsync(string userId, string? period = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<EditExpenseViewModel?> GetExpenseForEditAsync(int id, string userId);
        Task<ExpenseListItemViewModel?> GetExpenseDetailsAsync(int id, string userId);
        Task<bool> CreateExpenseAsync(CreateExpenseViewModel model, string userId);
        Task<bool> UpdateExpenseAsync(EditExpenseViewModel model, string userId);
        Task<bool> DeleteExpenseAsync(int id, string userId);
        Task<List<ExpenseCategoryViewModel>> GetCategoriesAsync(string userId);
        Task<EditExpenseCategoryViewModel?> GetCategoryForEditAsync(int id, string userId);
        Task<ExpenseCategoryViewModel?> GetCategoryDetailsAsync(int id, string userId);
        Task<bool> CreateCategoryAsync(CreateExpenseCategoryViewModel model, string userId);
        Task<bool> UpdateCategoryAsync(EditExpenseCategoryViewModel model, string userId);
        Task<bool> DeleteCategoryAsync(int id, string userId);
        Task SeedDefaultCategoriesAsync(string userId);

        /// <summary>
        /// Sets budget for a category and recalculates remaining balance
        /// </summary>
        Task<bool> SetCategoryBudgetAsync(int categoryId, decimal budgetAmount, string userId);
    }
}
