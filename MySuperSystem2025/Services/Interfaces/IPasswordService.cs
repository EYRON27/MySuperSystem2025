using MySuperSystem2025.Models.ViewModels.Password;

namespace MySuperSystem2025.Services.Interfaces
{
    /// <summary>
    /// Password service interface for secure credential management
    /// </summary>
    public interface IPasswordService
    {
        Task<PasswordDashboardViewModel> GetDashboardAsync(string userId, int? categoryId = null, string? searchTerm = null);
        Task<EditPasswordViewModel?> GetPasswordForEditAsync(int id, string userId);
        Task<PasswordListItemViewModel?> GetPasswordForDisplayAsync(int id, string userId);
        Task<bool> CreatePasswordAsync(CreatePasswordViewModel model, string userId);
        Task<bool> UpdatePasswordAsync(EditPasswordViewModel model, string userId);
        Task<bool> DeletePasswordAsync(int id, string userId);
        Task<string?> RevealPasswordAsync(int id, string userId);
        Task<List<PasswordCategoryViewModel>> GetCategoriesAsync(string userId);
        Task<EditPasswordCategoryViewModel?> GetCategoryForEditAsync(int id, string userId);
        Task<PasswordCategoryViewModel?> GetCategoryDetailsAsync(int id, string userId);
        Task<bool> CreateCategoryAsync(CreatePasswordCategoryViewModel model, string userId);
        Task<bool> UpdateCategoryAsync(EditPasswordCategoryViewModel model, string userId);
        Task<bool> DeleteCategoryAsync(int id, string userId);
        Task SeedDefaultCategoriesAsync(string userId);
    }
}
