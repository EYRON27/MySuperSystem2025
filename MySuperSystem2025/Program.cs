using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySuperSystem2025.Data;
using MySuperSystem2025.Middleware;
using MySuperSystem2025.Models.Domain;
using MySuperSystem2025.Repositories;
using MySuperSystem2025.Repositories.Interfaces;
using MySuperSystem2025.Services;
using MySuperSystem2025.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("Starting database initialization...");
        
        // Check if tables exist by trying to query
        bool tablesExist = false;
        try
        {
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers'";
            var result = await cmd.ExecuteScalarAsync();
            tablesExist = Convert.ToInt32(result) > 0;
            await conn.CloseAsync();
        }
        catch
        {
            tablesExist = false;
        }
        
        if (!tablesExist)
        {
            logger.LogInformation("Tables do not exist. Recreating database...");
            
            // Delete the database if it exists (it's in a bad state)
            await db.Database.EnsureDeletedAsync();
            logger.LogInformation("Old database deleted.");
            
            // Create fresh database with all tables
            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("? Database and tables created successfully.");
        }
        else
        {
            logger.LogInformation("? Database and tables already exist.");
        }
        
        // Seed roles
        logger.LogInformation("Seeding database with initial data...");
        await DatabaseSeeder.SeedAsync(services);
        
        logger.LogInformation("? Database initialization completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? ERROR: Failed to initialize database: {Message}", ex.Message);
        throw; // Fail fast - don't let app run with broken database
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseGlobalExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
