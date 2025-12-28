using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Models.ViewModels.Password;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services.Interfaces;

namespace MySuperSystem2025.Services
{
    /// <summary>
    /// Password service implementation handling secure credential storage.
    /// All passwords are encrypted using AES encryption.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            IUnitOfWork unitOfWork, 
            IEncryptionService encryptionService,
            ILogger<PasswordService> logger)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard data with stored passwords (masked)
        /// </summary>
        public async Task<PasswordDashboardViewModel> GetDashboardAsync(string userId, int? categoryId = null, string? searchTerm = null)
        {
            IEnumerable<StoredPassword> passwords;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                passwords = await _unitOfWork.Passwords.SearchUserPasswordsAsync(userId, searchTerm);
            }
            else if (categoryId.HasValue)
            {
                passwords = await _unitOfWork.Passwords.GetUserPasswordsByCategoryAsync(userId, categoryId.Value);
            }
            else
            {
                passwords = await _unitOfWork.Passwords.GetUserPasswordsAsync(userId);
            }

            var categories = await GetCategoriesAsync(userId);

            return new PasswordDashboardViewModel
            {
                TotalPasswords = passwords.Count(),
                Passwords = passwords.Select(MapToListItem).ToList(),
                Categories = categories,
                FilterCategoryId = categoryId,
                SearchTerm = searchTerm
            };
        }

        /// <summary>
        /// Gets a password for editing (without decrypting)
        /// </summary>
        public async Task<EditPasswordViewModel?> GetPasswordForEditAsync(int id, string userId)
        {
            var password = await _unitOfWork.Passwords.GetPasswordWithCategoryAsync(id, userId);
            if (password == null) return null;

            return new EditPasswordViewModel
            {
                Id = password.Id,
                WebsiteOrAppName = password.WebsiteOrAppName,
                WebsiteUrl = password.WebsiteUrl,
                Username = password.Username,
                CategoryId = password.CategoryId,
                Notes = password.Notes
            };
        }

        /// <summary>
        /// Gets a password for display purposes (masked)
        /// </summary>
        public async Task<PasswordListItemViewModel?> GetPasswordForDisplayAsync(int id, string userId)
        {
            var password = await _unitOfWork.Passwords.GetPasswordWithCategoryAsync(id, userId);
            if (password == null) return null;

            return MapToListItem(password);
        }

        /// <summary>
        /// Creates a new stored password (encrypts the password)
        /// </summary>
        public async Task<bool> CreatePasswordAsync(CreatePasswordViewModel model, string userId)
        {
            try
            {
                // Encrypt the password before storing
                var encryptedPassword = _encryptionService.Encrypt(model.Password);

                var storedPassword = new StoredPassword
                {
                    WebsiteOrAppName = model.WebsiteOrAppName,
                    WebsiteUrl = model.WebsiteUrl,
                    Username = model.Username,
                    EncryptedPassword = encryptedPassword,
                    CategoryId = model.CategoryId,
                    Notes = model.Notes,
                    UserId = userId
                };

                await _unitOfWork.Passwords.AddAsync(storedPassword);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password stored successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing password for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates a stored password
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(EditPasswordViewModel model, string userId)
        {
            try
            {
                var storedPassword = await _unitOfWork.Passwords.GetPasswordByIdAndUserAsync(model.Id, userId);
                if (storedPassword == null) return false;

                storedPassword.WebsiteOrAppName = model.WebsiteOrAppName;
                storedPassword.WebsiteUrl = model.WebsiteUrl;
                storedPassword.Username = model.Username;
                storedPassword.CategoryId = model.CategoryId;
                storedPassword.Notes = model.Notes;

                // Only update password if a new one is provided
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    storedPassword.EncryptedPassword = _encryptionService.Encrypt(model.NewPassword);
                }

                _unitOfWork.Passwords.Update(storedPassword);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password {PasswordId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password {PasswordId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a stored password
        /// </summary>
        public async Task<bool> DeletePasswordAsync(int id, string userId)
        {
            try
            {
                var storedPassword = await _unitOfWork.Passwords.GetPasswordByIdAndUserAsync(id, userId);
                if (storedPassword == null) return false;

                storedPassword.IsDeleted = true;
                storedPassword.DeletedAt = DateTime.UtcNow;

                _unitOfWork.Passwords.Update(storedPassword);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password {PasswordId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting password {PasswordId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Reveals (decrypts) a stored password
        /// Should only be called after re-authentication
        /// </summary>
        public async Task<string?> RevealPasswordAsync(int id, string userId)
        {
            try
            {
                var storedPassword = await _unitOfWork.Passwords.GetPasswordByIdAndUserAsync(id, userId);
                if (storedPassword == null) return null;

                var decryptedPassword = _encryptionService.Decrypt(storedPassword.EncryptedPassword);
                
                _logger.LogInformation("Password {PasswordId} revealed for user {UserId}", id, userId);
                return decryptedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revealing password {PasswordId} for user {UserId}", id, userId);
                return null;
            }
        }

        /// <summary>
        /// Gets all password categories for a user
        /// </summary>
        public async Task<List<PasswordCategoryViewModel>> GetCategoriesAsync(string userId)
        {
            var categories = await _unitOfWork.PasswordCategories.GetUserCategoriesAsync(userId);
            return categories.Select(c => new PasswordCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsDefault = c.IsDefault,
                PasswordCount = c.StoredPasswords.Count
            }).ToList();
        }

        /// <summary>
        /// Gets a category for editing
        /// </summary>
        public async Task<EditPasswordCategoryViewModel?> GetCategoryForEditAsync(int id, string userId)
        {
            var category = await _unitOfWork.PasswordCategories.GetCategoryWithPasswordsAsync(id, userId);
            if (category == null) return null;

            return new EditPasswordCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault
            };
        }

        /// <summary>
        /// Gets category details for display/deletion
        /// </summary>
        public async Task<PasswordCategoryViewModel?> GetCategoryDetailsAsync(int id, string userId)
        {
            var category = await _unitOfWork.PasswordCategories.GetCategoryWithPasswordsAsync(id, userId);
            if (category == null) return null;

            return new PasswordCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsDefault = category.IsDefault,
                PasswordCount = category.StoredPasswords.Count
            };
        }

        /// <summary>
        /// Creates a new password category
        /// </summary>
        public async Task<bool> CreateCategoryAsync(CreatePasswordCategoryViewModel model, string userId)
        {
            try
            {
                if (await _unitOfWork.PasswordCategories.CategoryNameExistsAsync(userId, model.Name))
                {
                    _logger.LogWarning("Password category name already exists for user {UserId}", userId);
                    return false;
                }

                var category = new PasswordCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    UserId = userId,
                    IsDefault = false
                };

                await _unitOfWork.PasswordCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password category created for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating password category for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Updates a password category
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(EditPasswordCategoryViewModel model, string userId)
        {
            try
            {
                var category = await _unitOfWork.PasswordCategories.GetCategoryWithPasswordsAsync(model.Id, userId);
                if (category == null) return false;

                if (await _unitOfWork.PasswordCategories.CategoryNameExistsAsync(userId, model.Name, model.Id))
                {
                    _logger.LogWarning("Password category name already exists for user {UserId}", userId);
                    return false;
                }

                category.Name = model.Name;
                category.Description = model.Description;

                _unitOfWork.PasswordCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password category {CategoryId} updated for user {UserId}", model.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password category {CategoryId} for user {UserId}", model.Id, userId);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a password category (allows deletion even with passwords)
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            try
            {
                var category = await _unitOfWork.PasswordCategories.GetCategoryWithPasswordsAsync(id, userId);
                if (category == null) return false;

                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;

                _unitOfWork.PasswordCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password category {CategoryId} deleted for user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting password category {CategoryId} for user {UserId}", id, userId);
                return false;
            }
        }

        /// <summary>
        /// Seeds default password categories for a new user
        /// </summary>
        public async Task SeedDefaultCategoriesAsync(string userId)
        {
            var defaultCategories = new[]
            {
                new PasswordCategory { Name = "Social", Description = "Social media accounts", UserId = userId, IsDefault = true },
                new PasswordCategory { Name = "Banking", Description = "Banking and financial accounts", UserId = userId, IsDefault = true },
                new PasswordCategory { Name = "Work", Description = "Work related accounts", UserId = userId, IsDefault = true }
            };

            foreach (var category in defaultCategories)
            {
                if (!await _unitOfWork.PasswordCategories.CategoryNameExistsAsync(userId, category.Name))
                {
                    await _unitOfWork.PasswordCategories.AddAsync(category);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Default password categories seeded for user {UserId}", userId);
        }

        private static PasswordListItemViewModel MapToListItem(StoredPassword password)
        {
            return new PasswordListItemViewModel
            {
                Id = password.Id,
                WebsiteOrAppName = password.WebsiteOrAppName,
                WebsiteUrl = password.WebsiteUrl,
                Username = password.Username,
                MaskedPassword = "••••••••",
                CategoryName = password.Category?.Name ?? "Uncategorized",
                CategoryId = password.CategoryId,
                Notes = password.Notes,
                CreatedAt = password.CreatedAt
            };
        }
    }
}
