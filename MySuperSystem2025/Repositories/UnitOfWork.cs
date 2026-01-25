using MySuperSystem2025.Data;
using MySuperSystem2025.Repositories.Interfaces;

namespace MySuperSystem2025.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing transactions and repositories.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IExpenseRepository? _expenses;
        private IExpenseCategoryRepository? _expenseCategories;
        private ITaskRepository? _tasks;
        private IPasswordRepository? _passwords;
        private IPasswordCategoryRepository? _passwordCategories;
        private ITimeEntryRepository? _timeEntries;
        private ITimeCategoryRepository? _timeCategories;
        private IFoodEntryRepository? _foodEntries;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IExpenseRepository Expenses => 
            _expenses ??= new ExpenseRepository(_context);

        public IExpenseCategoryRepository ExpenseCategories => 
            _expenseCategories ??= new ExpenseCategoryRepository(_context);

        public ITaskRepository Tasks => 
            _tasks ??= new TaskRepository(_context);

        public IPasswordRepository Passwords => 
            _passwords ??= new PasswordRepository(_context);

        public IPasswordCategoryRepository PasswordCategories => 
            _passwordCategories ??= new PasswordCategoryRepository(_context);

        public ITimeEntryRepository TimeEntries => 
            _timeEntries ??= new TimeEntryRepository(_context);

        public ITimeCategoryRepository TimeCategories => 
            _timeCategories ??= new TimeCategoryRepository(_context);

        public IFoodEntryRepository FoodEntries => 
            _foodEntries ??= new FoodEntryRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
