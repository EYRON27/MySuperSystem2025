# MySuperSystem2025 - Full MVC Management System

A comprehensive ASP.NET Core MVC application built with .NET 8, featuring expense tracking, task management, and secure password storage.

## ?? Features

### ?? Authentication & Authorization
- **User Registration & Login** with strong password requirements
- ASP.NET Core Identity for secure authentication
- Password requirements:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 number
  - At least 1 special character
- Account lockout after 5 failed attempts
- Role-based access control

### ?? Expense Management System
- **Dashboard Overview** with summaries:
  - Today's expenses
  - Weekly expenses
  - Monthly expenses
  - Yearly expenses
- **Category-based Organization**:
  - Default categories: Business, Personal, Personal Business
  - Create custom categories
  - Edit and delete custom categories (with validation)
- **Expense Operations**:
  - Add new expenses with amount, date, category, and description
  - Edit existing expenses
  - Delete expenses (soft delete)
  - Filter by period (daily, weekly, monthly, yearly)
  - Filter by category
- **Data Validation**:
  - Amount must be greater than 0
  - Date cannot be in the future
  - Description required (max 255 characters)
  - Category name validation (letters, numbers, spaces only)

### ? Task Management System
- **Kanban-style Dashboard** with three columns:
  - To Do
  - Ongoing
  - Completed
- **Task Operations**:
  - Create tasks with title, description, and deadline
  - Edit tasks (completed tasks are read-only)
  - Update task status with drag-and-drop workflow
  - Delete tasks
- **Task Status Tracking**:
  - Pending tasks counter
  - Completed tasks counter
  - Overdue tasks warning
- **Data Validation**:
  - Title required (max 100 characters)
  - Description optional (max 300 characters)
  - Deadline cannot be in the past
  - Completed tasks cannot be edited

### ?? Password Manager
- **Secure Password Storage**:
  - AES-256 encryption for all stored passwords
  - Passwords never stored in plain text
  - Password masking by default
- **Password Operations**:
  - Store credentials with website/app name, username, password, and notes
  - Edit stored passwords
  - Delete passwords (soft delete)
  - Search passwords by website, username, or notes
  - Filter by category
- **Security Features**:
  - Re-authentication required to reveal passwords
  - Modern Clipboard API for secure copy-to-clipboard
  - Password strength requirements (min 8 characters)
- **Category Management**:
  - Default categories: Social, Banking, Work
  - Create custom categories
  - Edit and delete custom categories

## ??? Architecture

### Clean Architecture
The project follows clean architecture principles with clear separation of concerns:

```
MySuperSystem2025/
??? Controllers/          # MVC Controllers
??? Models/
?   ??? Domain/          # Entity models
?   ??? ViewModels/      # View models
??? Views/               # Razor views
??? Services/            # Business logic layer
?   ??? Interfaces/      # Service contracts
??? Repositories/        # Data access layer
?   ??? Interfaces/      # Repository contracts
??? Data/                # DbContext and seeding
??? Middleware/          # Custom middleware
??? wwwroot/            # Static files
```

### Design Patterns
- **Repository Pattern**: Abstraction over data access
- **Unit of Work Pattern**: Transaction management
- **Service Layer Pattern**: Business logic encapsulation
- **Dependency Injection**: Loose coupling
- **SOLID Principles**: Throughout the codebase

## ??? Tech Stack

- **Framework**: ASP.NET Core MVC 8.0
- **Authentication**: ASP.NET Core Identity
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server (LocalDB)
- **Logging**: Serilog
- **Encryption**: AES-256 for password storage
- **UI Framework**: Bootstrap 5
- **Icons**: Bootstrap Icons

## ?? NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

## ?? Configuration

### Database Connection String
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MySuperSystem2025;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Encryption Key
**IMPORTANT**: Change the encryption key in production:
```json
{
  "Encryption": {
    "Key": "MySuperSecretEncryptionKey2025!@#$%^&*()_+=-"
  }
}
```

## ?? Getting Started

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- SQL Server LocalDB

### Installation Steps

1. **Clone or open the project in Visual Studio 2022**

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Update the database**:
   The migrations will run automatically on application startup, or manually run:
   ```bash
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```
   Or press F5 in Visual Studio

5. **Register a new account**:
   - Navigate to `/Account/Register`
   - Fill in your details
   - Default categories will be seeded automatically

## ??? Database Schema

### Key Entities
- **ApplicationUser**: Extended Identity user with navigation properties
- **Expense**: Financial transaction records
- **ExpenseCategory**: Expense categorization
- **TaskItem**: Task/to-do items with status tracking
- **StoredPassword**: Encrypted credential storage
- **PasswordCategory**: Password categorization

### Key Features
- **Soft Delete**: All entities support soft delete (IsDeleted flag)
- **Audit Fields**: CreatedAt, UpdatedAt, DeletedAt timestamps
- **Query Filters**: Automatic filtering of soft-deleted records
- **Indexes**: Optimized for common queries
- **Foreign Keys**: Referential integrity with restrict delete behavior

## ?? Security Features

### Authentication
- Strong password requirements enforced
- Account lockout protection
- Secure cookie-based authentication
- HTTPS required in production

### Data Protection
- **Password Hashing**: ASP.NET Core Identity with PBKDF2
- **Encryption**: AES-256 for stored passwords
- **SQL Injection Prevention**: Parameterized queries via EF Core
- **XSS Prevention**: Built-in Razor encoding
- **CSRF Protection**: Anti-forgery tokens on all forms
- **Re-authentication**: Required for sensitive operations

### Input Validation
- Server-side validation with Data Annotations
- Client-side validation with jQuery Validation
- Model state validation in controllers
- Business rule validation in service layer

## ?? Dashboard Features

### Expense Dashboard
- Real-time expense summaries
- Category breakdown with percentages
- Recent expense list
- Quick filters by period and category

### Task Dashboard
- Visual task organization (To Do, Ongoing, Completed)
- Overdue task alerts
- Quick status updates
- Task statistics

### Password Manager Dashboard
- Encrypted password list
- Search functionality
- Category filtering
- Secure password reveal with re-authentication

## ?? UI/UX Features

- **Responsive Design**: Mobile-friendly layout
- **Sidebar Navigation**: Easy access to all modules
- **Modern Design**: Clean, professional interface
- **Bootstrap 5**: Modern component library
- **Toast Notifications**: Success/error messages
- **Loading States**: User feedback during operations
- **Form Validation**: Real-time feedback
- **Card-based Layout**: Organized information display

## ?? Logging

Logs are written to:
- **Console**: For development
- **File**: `logs/app-{date}.log` with daily rolling

Log levels configured in `appsettings.json`.

## ?? Future Enhancements

The system is ready for:
- RESTful API expansion
- Export functionality (CSV, PDF)
- Email notifications
- Multi-language support
- Dark mode
- Mobile app integration
- Advanced reporting and analytics
- Budget planning features
- Recurring expenses/tasks
- Data backup and restore

## ?? Testing

The architecture supports easy testing:
- **Unit Tests**: Service layer with mocked repositories
- **Integration Tests**: Controllers with in-memory database
- **End-to-End Tests**: Full application flow

## ?? License

This is a demonstration project for educational purposes.

## ????? Development

### Code Structure
- **Controllers**: Thin controllers delegating to services
- **Services**: Business logic and orchestration
- **Repositories**: Data access and queries
- **ViewModels**: View-specific data transfer
- **Domain Models**: Database entities

### Coding Standards
- Clear naming conventions
- XML documentation comments
- SOLID principles
- Dependency injection
- Async/await patterns
- Exception handling
- Logging throughout

## ?? Contributing

This is a complete, production-ready system demonstrating best practices in ASP.NET Core MVC development.

---

**Built with ?? using ASP.NET Core MVC and Clean Architecture**
