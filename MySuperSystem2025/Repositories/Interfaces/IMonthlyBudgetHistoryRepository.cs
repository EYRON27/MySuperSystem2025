using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    public interface IMonthlyBudgetHistoryRepository : IRepository<MonthlyBudgetHistory>
    {
        Task<MonthlyBudgetHistory?> GetHistoryAsync(string userId, int categoryId, int year, int month);
        Task<List<MonthlyBudgetHistory>> GetHistoriesForMonthAsync(string userId, int year, int month);
    }
}
