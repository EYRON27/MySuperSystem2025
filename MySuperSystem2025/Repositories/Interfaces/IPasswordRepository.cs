using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Password repository interface for secure credential storage
    /// </summary>
    public interface IPasswordRepository : IRepository<StoredPassword>
    {
        Task<IEnumerable<StoredPassword>> GetUserPasswordsAsync(string userId);
        Task<IEnumerable<StoredPassword>> GetUserPasswordsByCategoryAsync(string userId, int categoryId);
        Task<IEnumerable<StoredPassword>> SearchUserPasswordsAsync(string userId, string searchTerm);
        Task<StoredPassword?> GetPasswordByIdAndUserAsync(int id, string userId);
        Task<StoredPassword?> GetPasswordWithCategoryAsync(int id, string userId);
    }
}
