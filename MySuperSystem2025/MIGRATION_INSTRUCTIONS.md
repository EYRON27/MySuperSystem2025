# Database Migration Instructions

## Creating the Initial Migration

If you need to create a fresh migration, follow these steps:

### Using Package Manager Console (Visual Studio)

1. Open **Package Manager Console** (Tools ? NuGet Package Manager ? Package Manager Console)

2. Create the initial migration:
```powershell
Add-Migration InitialCreate
```

3. Apply the migration to create the database:
```powershell
Update-Database
```

### Using .NET CLI

1. Open a terminal in the project directory

2. Install EF Core tools (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

3. Create the initial migration:
```bash
dotnet ef migrations add InitialCreate
```

4. Apply the migration:
```bash
dotnet ef database update
```

## Automatic Migration

The application is configured to automatically apply migrations on startup via:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DatabaseSeeder.SeedAsync(app.Services);
}
```

## Verifying the Database

After running the application, verify the database exists:

1. In Visual Studio, go to **View ? SQL Server Object Explorer**
2. Expand **(localdb)\MSSQLLocalDB ? Databases**
3. You should see **MySuperSystem2025** database

## Database Tables Created

The migration will create the following tables:

### Identity Tables (ASP.NET Core Identity)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- AspNetRoleClaims

### Application Tables
- Expenses
- ExpenseCategories
- Tasks
- StoredPasswords
- PasswordCategories

## Seeded Data

On first user registration, the following data is automatically seeded:

### Expense Categories (per user)
- Business
- Personal
- Personal Business

### Password Categories (per user)
- Social
- Banking
- Work

### Roles (global)
- Admin
- User

## Troubleshooting

### Error: "Unable to create database"
- Ensure SQL Server LocalDB is installed
- Check the connection string in appsettings.json
- Run Visual Studio as Administrator

### Error: "A network-related or instance-specific error"
- Start SQL Server LocalDB: `sqllocaldb start mssqllocaldb`
- Or restart it: `sqllocaldb stop mssqllocaldb` then `sqllocaldb start mssqllocaldb`

### Error: "Login failed for user"
- The connection uses Windows Authentication (Trusted_Connection=True)
- Ensure your Windows account has access

### Reset Database
To start fresh:
```bash
# Drop the database
dotnet ef database drop

# Recreate it
dotnet ef database update
```

Or delete from SQL Server Object Explorer in Visual Studio.

## Production Deployment

For production deployment:

1. **Update connection string** in appsettings.Production.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MySuperSystem2025;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

2. **Change encryption key** in appsettings.Production.json:
```json
{
  "Encryption": {
    "Key": "YourStrongEncryptionKey!@#$%"
  }
}
```

3. **Apply migrations** on the production server:
```bash
dotnet ef database update --connection "YOUR_CONNECTION_STRING"
```

## Additional Commands

### List all migrations
```bash
dotnet ef migrations list
```

### Remove last migration (if not applied)
```bash
dotnet ef migrations remove
```

### Generate SQL script
```bash
dotnet ef migrations script
```

### Generate SQL script for specific migration range
```bash
dotnet ef migrations script InitialCreate AddNewFeature
```
