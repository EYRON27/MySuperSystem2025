-- Run this in SQL Server Management Studio or Visual Studio SQL Server Object Explorer
-- This will delete the database if it exists in a bad state

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'MySuperSystem2025')
BEGIN
    ALTER DATABASE MySuperSystem2025 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MySuperSystem2025;
    PRINT 'Database MySuperSystem2025 has been deleted.';
END
ELSE
BEGIN
    PRINT 'Database MySuperSystem2025 does not exist.';
END
GO
