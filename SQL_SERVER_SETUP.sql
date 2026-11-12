-- ============================================================================
-- STOM Database Setup Script
-- ============================================================================
-- This script documents the database setup for STOM.
-- NOTE: In normal operation, the database is created automatically by the 
--       Entity Framework Core migrator when STOM.API starts.
-- 
-- This script is provided for reference and manual setup if needed.
-- ============================================================================

-- ============================================================================
-- For Development with LocalDB
-- ============================================================================
-- LocalDB is automatically installed with Visual Studio and SQL Server Developer Edition.
-- 
-- To use LocalDB, no manual setup is required. The application will:
--   1. Connect to: (localdb)\mssqllocaldb
--   2. Create database: STOM_DB (or STOM_DB_Dev for development)
--   3. Apply all migrations automatically
-- 
-- To connect to LocalDB in Management Studio:
--   Server name: (localdb)\mssqllocaldb
-- 
-- To view LocalDB instances:
--   Run in PowerShell: sqllocaldb info
-- 
-- ============================================================================
-- For Production with SQL Server
-- ============================================================================
-- For production deployments using full SQL Server, follow these steps:

-- 1. Create the database (if not using automatic creation)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'STOM_DB')
BEGIN
    CREATE DATABASE [STOM_DB]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO

-- 2. Create login (example using SQL Server Authentication)
-- IMPORTANT: Change the password to a strong password in production!
IF NOT EXISTS (SELECT * FROM sys.syslogins WHERE name = N'stom_user')
BEGIN
    CREATE LOGIN [stom_user] WITH PASSWORD = N'ChangeMe!SecurePassword123';
END
GO

-- 3. Create user in database and grant permissions
USE [STOM_DB]
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'stom_user')
BEGIN
    CREATE USER [stom_user] FOR LOGIN [stom_user];
END
GO

-- Grant necessary permissions to the user
ALTER ROLE [db_owner] ADD MEMBER [stom_user];
GO

-- ============================================================================
-- Database Configuration
-- ============================================================================
-- Set appropriate database options for STOM

-- Enable recovery model (SIMPLE for development, FULL for production)
ALTER DATABASE [STOM_DB] SET RECOVERY SIMPLE;
GO

-- Set auto-growth for data files (adjust as needed)
ALTER DATABASE [STOM_DB] MODIFY FILE (NAME = N'STOM_DB', FILEGROWTH = 10MB);
GO

-- Enable query optimization
ALTER DATABASE [STOM_DB] SET QUERY_STORE = ON;
GO

-- ============================================================================
-- Verification
-- ============================================================================
-- Verify database creation and setup

-- Check database exists
SELECT name, state_desc, recovery_model_desc
FROM sys.databases
WHERE name = N'STOM_DB';

-- Check users and their roles
USE [STOM_DB]
GO
SELECT name, type_desc
FROM sys.database_principals
WHERE name = N'stom_user';

-- ============================================================================
-- Automatic Migration Execution
-- ============================================================================
-- When STOM.API starts, it automatically:
--
-- 1. Connects to the database using the connection string from appsettings.json:
--    Server=(localdb)\mssqllocaldb;Database=STOM_DB;Integrated Security=true;
--
-- 2. Ensures the database exists (creates it if necessary)
--
-- 3. Runs all pending migrations from STOM.API/Migrations/
--    - Initial migration creates tables: Games, Scores, Tournaments, etc.
--    - Applies any new migrations if they've been added
--
-- 4. Logs migration progress to the application logs
--
-- No manual SQL execution needed for normal operation!

-- ============================================================================
-- Manual Migration (if needed)
-- ============================================================================
-- To manually apply migrations from the command line:
--
-- cd C:\Source\Repos\STOM\STOM.API
-- 
-- Apply all pending migrations:
-- dotnet ef database update
--
-- Roll back to a specific migration:
-- dotnet ef database update <MigrationName>
--
-- List all migrations:
-- dotnet ef migrations list
--
-- Create a new migration (after model changes):
-- dotnet ef migrations add <MigrationName>

-- ============================================================================
-- Backup and Recovery
-- ============================================================================
-- Example backup script (for production)

-- Full database backup
-- BACKUP DATABASE [STOM_DB] 
-- TO DISK = N'C:\Backups\STOM_DB_Full_Backup.bak'
-- WITH INIT, COMPRESSION;

-- Transaction log backup (for transaction log recovery)
-- BACKUP LOG [STOM_DB]
-- TO DISK = N'C:\Backups\STOM_DB_Log_Backup.trn'
-- WITH INIT;

-- ============================================================================
-- Troubleshooting
-- ============================================================================
-- If database creation fails:
--
-- 1. Check LocalDB is running:
--    sqllocaldb start
--
-- 2. Check application logs for specific error messages
--
-- 3. Verify connection string in appsettings.json
--
-- 4. Ensure SQL Server permissions are correct (Integrated Security)
--
-- 5. For production, verify SQL login/password is correct
--
-- 6. Check SQL Server is accepting connections on the configured port

