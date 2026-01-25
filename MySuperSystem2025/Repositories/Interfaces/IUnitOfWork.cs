namespace MySuperSystem2025.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work interface for managing transactions and repositories.
    /// Ensures all changes are committed together or rolled back.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IExpenseRepository Expenses { get; }
        IExpenseCategoryRepository ExpenseCategories { get; }
        ITaskRepository Tasks { get; }
        IPasswordRepository Passwords { get; }
        IPasswordCategoryRepository PasswordCategories { get; }
        ITimeEntryRepository TimeEntries { get; }
        ITimeCategoryRepository TimeCategories { get; }
        IFoodEntryRepository FoodEntries { get; }

        Task<int> SaveChangesAsync();
    }
}
