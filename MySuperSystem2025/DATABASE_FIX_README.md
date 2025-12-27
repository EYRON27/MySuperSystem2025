# Database Setup Instructions for MySuperSystem2025

## The Issue
You were getting the error: `Invalid object name 'AspNetRoles'` because the database tables haven't been created yet.

## Solution - Multiple Options

### Option 1: Automatic Database Creation (EASIEST - RECOMMENDED)
1. **STOP the debugger** if it's running (click the red square or press Shift+F5)
2. Make sure your connection string in `appsettings.json` is correct
3. **Start the application again** (press F5)
4. The database will be automatically created with all tables!

The code has been updated to use `EnsureCreatedAsync()` which will:
- Create the database if it doesn't exist
- Create all tables based on your models
- Run the seeder to create Admin and User roles

### Option 2: Using Entity Framework Migrations (PROPER WAY)
If you want to use migrations (recommended for production):

1. **STOP the debugger**
2. Open **Package Manager Console** (Tools ? NuGet Package Manager ? Package Manager Console)
3. Run these commands:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```
4. Start your application

### Option 3: Run SQL Script Manually
If the above doesn't work, you can run the SQL script manually:

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server
3. Create database if needed: `CREATE DATABASE MySuperSystem2025`
4. Open the file `DatabaseSetup.sql` (located in this project folder)
5. Execute the script against your database
6. Start your application

## What Was Fixed

### Changes Made:
1. **Program.cs**: Changed from `db.Database.Migrate()` to `db.Database.EnsureCreatedAsync()`
   - This creates the database and tables automatically if they don't exist
   - Added proper error handling and logging

2. **DatabaseSeeder.cs**: Removed redundant scope creation
   - Now uses the scope passed from Program.cs

3. **DatabaseSetup.sql**: Created a comprehensive SQL script
   - Contains all Identity tables (AspNetRoles, AspNetUsers, etc.)
   - Contains all application tables (Expenses, Tasks, StoredPasswords, etc.)
   - Includes default Admin and User roles

## Verify Your Connection String

Make sure your `appsettings.json` has a valid connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MySuperSystem2025;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Or for SQL Server Express:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MySuperSystem2025;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

## Next Steps

1. Stop the debugger
2. Press F5 to start the application
3. The database will be created automatically
4. You should now be able to register and login!

## Troubleshooting

If you still get errors:
- Check the Output window in Visual Studio for detailed error messages
- Verify SQL Server is running
- Verify your connection string is correct
- Check if you have permissions to create databases
