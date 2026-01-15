using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Models.Domain;

namespace MySuperSystem2025.Data
{
    /// <summary>
    /// Application database context extending Identity for authentication.
    /// Implements soft delete pattern through query filters.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Expense Management
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }

        // Task Management
        public DbSet<TaskItem> Tasks { get; set; }

        // Password Management
        public DbSet<StoredPassword> StoredPasswords { get; set; }
        public DbSet<PasswordCategory> PasswordCategories { get; set; }

        // Time Tracking
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<TimeCategory> TimeCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure soft delete query filters
            builder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ExpenseCategory>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<TaskItem>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<StoredPassword>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<PasswordCategory>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<TimeEntry>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<TimeCategory>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);

            // Configure relationships for Expense
            builder.Entity<Expense>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Expenses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Expenses)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.UserId);
            });

            // Configure relationships for ExpenseCategory
            builder.Entity<ExpenseCategory>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.ExpenseCategories)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            });

            // Configure relationships for TaskItem
            builder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.User)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.Deadline);
            });

            // Configure relationships for StoredPassword
            builder.Entity<StoredPassword>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithMany(u => u.StoredPasswords)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.StoredPasswords)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.UserId);
            });

            // Configure relationships for PasswordCategory
            builder.Entity<PasswordCategory>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.PasswordCategories)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
            });

            // Configure relationships for TimeEntry
            builder.Entity<TimeEntry>(entity =>
            {
                entity.HasOne(t => t.User)
                    .WithMany(u => u.TimeEntries)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Category)
                    .WithMany(c => c.TimeEntries)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.StartTime);
                entity.HasIndex(t => t.UserId);
            });

            // Configure relationships for TimeCategory
            builder.Entity<TimeCategory>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.TimeCategories)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically set audit fields
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically set audit fields
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
