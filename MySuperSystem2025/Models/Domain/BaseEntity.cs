using System.ComponentModel.DataAnnotations;

namespace MySuperSystem2025.Models.Domain
{
    /// <summary>
    /// Base entity class providing common properties for all domain entities.
    /// Implements soft delete pattern and audit fields.
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag - entities are never permanently deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
