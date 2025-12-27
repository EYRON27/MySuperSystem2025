-- Database Setup Script for MySuperSystem2025
-- Run this script in your SQL Server to create all necessary tables

USE [MySuperSystem2025]
GO

-- Create AspNetRoles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetRoles](
        [Id] [nvarchar](450) NOT NULL,
        [Name] [nvarchar](256) NULL,
        [NormalizedName] [nvarchar](256) NULL,
        [ConcurrencyStamp] [nvarchar](max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
    (
        [NormalizedName] ASC
    )
    WHERE [NormalizedName] IS NOT NULL
END
GO

-- Create AspNetUsers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [dbo].[AspNetUsers](
        [Id] [nvarchar](450) NOT NULL,
        [FirstName] [nvarchar](max) NULL,
        [LastName] [nvarchar](max) NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [UserName] [nvarchar](256) NULL,
        [NormalizedUserName] [nvarchar](256) NULL,
        [Email] [nvarchar](256) NULL,
        [NormalizedEmail] [nvarchar](256) NULL,
        [EmailConfirmed] [bit] NOT NULL,
        [PasswordHash] [nvarchar](max) NULL,
        [SecurityStamp] [nvarchar](max) NULL,
        [ConcurrencyStamp] [nvarchar](max) NULL,
        [PhoneNumber] [nvarchar](max) NULL,
        [PhoneNumberConfirmed] [bit] NOT NULL,
        [TwoFactorEnabled] [bit] NOT NULL,
        [LockoutEnd] [datetimeoffset](7) NULL,
        [LockoutEnabled] [bit] NOT NULL,
        [AccessFailedCount] [int] NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    
    CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]([NormalizedEmail] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]([NormalizedUserName] ASC)
    WHERE [NormalizedUserName] IS NOT NULL
END
GO

-- Create AspNetUserRoles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles](
        [UserId] [nvarchar](450) NOT NULL,
        [RoleId] [nvarchar](450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
            REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]([RoleId] ASC)
END
GO

-- Create AspNetUserClaims table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ClaimType] [nvarchar](max) NULL,
        [ClaimValue] [nvarchar](max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]([UserId] ASC)
END
GO

-- Create AspNetUserLogins table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins](
        [LoginProvider] [nvarchar](450) NOT NULL,
        [ProviderKey] [nvarchar](450) NOT NULL,
        [ProviderDisplayName] [nvarchar](max) NULL,
        [UserId] [nvarchar](450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]([UserId] ASC)
END
GO

-- Create AspNetUserTokens table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens](
        [UserId] [nvarchar](450) NOT NULL,
        [LoginProvider] [nvarchar](450) NOT NULL,
        [Name] [nvarchar](450) NOT NULL,
        [Value] [nvarchar](max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    )
END
GO

-- Create AspNetRoleClaims table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RoleId] [nvarchar](450) NOT NULL,
        [ClaimType] [nvarchar](max) NULL,
        [ClaimValue] [nvarchar](max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
            REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    )
    
    CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]([RoleId] ASC)
END
GO

-- Create ExpenseCategories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExpenseCategories')
BEGIN
    CREATE TABLE [dbo].[ExpenseCategories](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](450) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [Color] [nvarchar](max) NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ExpenseCategories_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ExpenseCategories_UserId_Name] ON [dbo].[ExpenseCategories]
    ([UserId] ASC, [Name] ASC)
END
GO

-- Create Expenses table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Expenses')
BEGIN
    CREATE TABLE [dbo].[Expenses](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Amount] [decimal](18, 2) NOT NULL,
        [Description] [nvarchar](max) NOT NULL,
        [Date] [datetime2](7) NOT NULL,
        [CategoryId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Expenses_ExpenseCategories_CategoryId] FOREIGN KEY([CategoryId])
            REFERENCES [dbo].[ExpenseCategories] ([Id]),
        CONSTRAINT [FK_Expenses_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    CREATE NONCLUSTERED INDEX [IX_Expenses_Date] ON [dbo].[Expenses]([Date] ASC)
    CREATE NONCLUSTERED INDEX [IX_Expenses_UserId] ON [dbo].[Expenses]([UserId] ASC)
    CREATE NONCLUSTERED INDEX [IX_Expenses_CategoryId] ON [dbo].[Expenses]([CategoryId] ASC)
END
GO

-- Create Tasks table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE [dbo].[Tasks](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](max) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [Status] [int] NOT NULL,
        [Priority] [int] NOT NULL,
        [Deadline] [datetime2](7) NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Tasks_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    CREATE NONCLUSTERED INDEX [IX_Tasks_UserId] ON [dbo].[Tasks]([UserId] ASC)
    CREATE NONCLUSTERED INDEX [IX_Tasks_Status] ON [dbo].[Tasks]([Status] ASC)
    CREATE NONCLUSTERED INDEX [IX_Tasks_Deadline] ON [dbo].[Tasks]([Deadline] ASC)
END
GO

-- Create PasswordCategories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordCategories')
BEGIN
    CREATE TABLE [dbo].[PasswordCategories](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](450) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [Color] [nvarchar](max) NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_PasswordCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PasswordCategories_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_PasswordCategories_UserId_Name] ON [dbo].[PasswordCategories]
    ([UserId] ASC, [Name] ASC)
END
GO

-- Create StoredPasswords table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StoredPasswords')
BEGIN
    CREATE TABLE [dbo].[StoredPasswords](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](max) NOT NULL,
        [Username] [nvarchar](max) NULL,
        [EncryptedPassword] [nvarchar](max) NOT NULL,
        [Website] [nvarchar](max) NULL,
        [Notes] [nvarchar](max) NULL,
        [CategoryId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [IsDeleted] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_StoredPasswords] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_StoredPasswords_PasswordCategories_CategoryId] FOREIGN KEY([CategoryId])
            REFERENCES [dbo].[PasswordCategories] ([Id]),
        CONSTRAINT [FK_StoredPasswords_AspNetUsers_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[AspNetUsers] ([Id])
    )
    
    CREATE NONCLUSTERED INDEX [IX_StoredPasswords_UserId] ON [dbo].[StoredPasswords]([UserId] ASC)
    CREATE NONCLUSTERED INDEX [IX_StoredPasswords_CategoryId] ON [dbo].[StoredPasswords]([CategoryId] ASC)
END
GO

-- Insert default roles
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
END

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'User')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'User', 'USER', NEWID())
END
GO

PRINT 'Database setup completed successfully!'
