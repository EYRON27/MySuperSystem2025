using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Password repository implementation for secure credential storage
    /// </summary>
    public class PasswordRepository : Repository<StoredPassword>, IPasswordRepository
    {
        public PasswordRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StoredPassword>> GetUserPasswordsAsync(string userId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.WebsiteOrAppName)
                .ToListAsync();
        }

        public async Task<IEnumerable<StoredPassword>> GetUserPasswordsByCategoryAsync(string userId, int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.UserId == userId && p.CategoryId == categoryId)
                .OrderBy(p => p.WebsiteOrAppName)
                .ToListAsync();
        }

        public async Task<IEnumerable<StoredPassword>> SearchUserPasswordsAsync(string userId, string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.UserId == userId && 
                           (p.WebsiteOrAppName.ToLower().Contains(lowerSearchTerm) ||
                            p.Username.ToLower().Contains(lowerSearchTerm) ||
                            (p.Notes != null && p.Notes.ToLower().Contains(lowerSearchTerm))))
                .OrderBy(p => p.WebsiteOrAppName)
                .ToListAsync();
        }

        public async Task<StoredPassword?> GetPasswordByIdAndUserAsync(int id, string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task<StoredPassword?> GetPasswordWithCategoryAsync(int id, string userId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }
    }
}
