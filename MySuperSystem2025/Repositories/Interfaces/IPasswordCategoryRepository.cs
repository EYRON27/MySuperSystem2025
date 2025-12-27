using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Password category repository interface
    /// </summary>
    public interface IPasswordCategoryRepository : IRepository<PasswordCategory>
    {
        Task<IEnumerable<PasswordCategory>> GetUserCategoriesAsync(string userId);
        Task<PasswordCategory?> GetCategoryWithPasswordsAsync(int id, string userId);
        Task<bool> CategoryNameExistsAsync(string userId, string name, int? excludeId = null);
    }
}
