# STOM Database Setup Guide

## Overview

STOM uses **SQL Server** as its primary database, with automatic database creation and schema management via Entity Framework Core migrations. The database is **automatically initialized on application startup** with zero manual setup required.

---

## Quick Start

### Prerequisites
- ✅ SQL Server Express LocalDB (included with Visual Studio or SQL Server Developer Edition)
- ✅ .NET 8+ runtime
- ✅ STOM.API and migrations already created

### To Start Using the Database

1. **Run the STOM.API application:**
   ```bash
   cd C:\Source\Repos\STOM\STOM.API
   dotnet run
   ```

2. **That's it!** The application automatically:
   - ✅ Creates the database if it doesn't exist
   - ✅ Applies all pending migrations
   - ✅ Logs progress to console
   - ✅ Is ready to accept requests

---

## Database Configuration

### Connection String

**File:** `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=STOM_DB;Integrated Security=true;"
  }
}
```

| Component | Value | Description |
|-----------|-------|-------------|
| **Server** | `(localdb)\mssqllocaldb` | Local SQL Server instance (no installation needed) |
| **Database** | `STOM_DB` | Database name (created automatically) |
| **Auth** | `Integrated Security=true` | Windows Authentication |

### Environment-Specific Settings

**Development:** `appsettings.Development.json`
- Database: `STOM_DB_Dev` (isolated from production)
- Logging: Debug level
- Auto-migrate: Enabled

**Production:** `appsettings.json`
- Database: `STOM_DB`
- Logging: Information level
- Auto-migrate: Enabled

---

## Automatic Database Initialization

### Startup Flow

When `STOM.API` starts, `Program.cs` automatically:

1. **Registers DbContext:**
   ```csharp
   builder.Services.AddDbContext<StomDbContext>(options =>
       options.UseSqlServer(...));
   ```

2. **Enables Retry Logic:**
   - Up to 3 connection attempts
   - 5-second delay between retries
   - Handles transient SQL Server connection failures

3. **Initializes Database:**
   ```csharp
   var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();
   await dbInitService.InitializeDatabaseAsync(dbContext);
   ```

4. **Executes Migrations:**
   - `dbContext.Database.EnsureCreated()` - Creates DB if missing
   - `dbContext.Database.Migrate()` - Applies all pending migrations
   - Logs each migration applied

5. **Verifies Schema:**
   - Checks that all migrations succeeded
   - Logs applied migration count
   - Ready for API requests

### Log Output Example

```
========== DATABASE INITIALIZATION START ==========
Using connection string: Server=(localdb)\mssqllocaldb;Database=STOM_DB;Integrated Security=true;
Ensuring database exists...
Database did not exist. Created new database successfully.
Found 1 pending migrations:
  - 20261109133537_InitialCreate
Applying pending migrations...
All pending migrations applied successfully.
Total applied migrations: 1
========== DATABASE INITIALIZATION SUCCESS ==========
```

---

## Migrations

### Current Migrations

Located in: `STOM.API/Migrations/`

| Migration | Name | Purpose |
|-----------|------|---------|
| `20261109133537_InitialCreate.cs` | Initial Create | Creates tables: Games, Scores, Tournaments, Competitions, Players, AuditLogs |

### Viewing Applied Migrations

**Using API:**
```bash
curl http://localhost:5000/api/health/database
```

Response:
```json
{
  "canConnect": true,
  "appliedMigrationsCount": 1,
  "appliedMigrations": ["20261109133537_InitialCreate"],
  "pendingMigrationsCount": 0,
  "pendingMigrations": [],
  "timestamp": "2024-11-12T22:00:00Z"
}
```

**Using dotnet-ef CLI:**
```bash
cd STOM.API
dotnet ef migrations list
```

---

## Manual Migration Management

### Creating a New Migration

After modifying the database model (e.g., adding a new entity):

```bash
cd C:\Source\Repos\STOM\STOM.API

# Generate migration file
dotnet ef migrations add AddNewFeature

# Apply to database
dotnet ef database update
```

### Rolling Back to a Previous Migration

```bash
cd STOM.API

# Revert to a specific migration
dotnet ef database update 20261109133537_InitialCreate

# Revert all migrations (dangerous - drops schema)
dotnet ef database update 0
```

### Removing a Migration (Before Applied)

```bash
cd STOM.API

# Remove the last migration file (if not applied to database)
dotnet ef migrations remove
```

---

## Database Verification

### Health Check Endpoints

#### 1. Simple Health Status
```bash
curl http://localhost:5000/api/health/status
```

**Success Response (200):**
```json
{
  "status": "Healthy",
  "message": "All systems operational",
  "timestamp": "2024-11-12T22:00:00Z"
}
```

**Failure Response (503):**
```json
{
  "status": "Unhealthy",
  "message": "Database connection failed",
  "timestamp": "2024-11-12T22:00:00Z"
}
```

#### 2. Detailed Database Diagnostics
```bash
curl http://localhost:5000/api/health/database
```

Response shows all applied and pending migrations.

### Manual Verification in SQL Server

**Connect to LocalDB:**
```
Server: (localdb)\mssqllocaldb
Authentication: Windows Authentication
```

**View Database:**
```sql
-- Show all databases
SELECT name FROM sys.databases WHERE name LIKE 'STOM%';

-- Show tables in STOM_DB
USE STOM_DB;
SELECT name FROM sys.tables;

-- Show applied migrations
SELECT * FROM dbo.__EFMigrationsHistory;
```

---

## Troubleshooting

### Issue: "Cannot connect to database"

**Cause:** LocalDB not running or not installed

**Solutions:**
```powershell
# Check if LocalDB is installed
sqllocaldb info

# Start LocalDB instance
sqllocaldb start

# View instance status
sqllocaldb info mssqllocaldb
```

### Issue: "Database already exists" error during creation

**Cause:** Database exists but migrations not applied

**Solution:**
```bash
cd STOM.API
dotnet ef database update
```

### Issue: Migration fails with "Invalid column name"

**Cause:** Stale migrations or model mismatch

**Solution:**
```bash
# Check pending migrations
dotnet ef migrations list

# View migration details
code STOM.API/Migrations/<MigrationName>.cs

# Remove and recreate if needed
dotnet ef migrations remove
dotnet ef migrations add <MigrationName>
```

### Issue: "Login failed" error

**Cause:** SQL Server authentication or permission issue

**For Development:** Ensure Windows Authentication is enabled and you're in the correct Windows user

**For Production:** Verify SQL login credentials in connection string

### Issue: Application starts but database is empty

**Cause:** Migrations didn't apply

**Solution:** Check application logs for initialization errors:
```bash
# Run with verbose logging
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --verbosity detailed
```

---

## Production Deployment

### Pre-Deployment Checklist

- ✅ All migrations created and tested locally
- ✅ Connection string updated for production SQL Server
- ✅ SQL Server login and permissions configured
- ✅ Database backup plan in place
- ✅ Network connectivity from app server to SQL Server verified

### Deployment Steps

1. **Update Connection String:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=production-sql.example.com;Database=STOM_DB;User Id=stom_user;Password=YourSecurePassword;"
     }
   }
   ```

2. **Deploy Application:**
   ```bash
   dotnet publish -c Release
   # Copy to production server
   ```

3. **Run Application:**
   - Database will be created automatically
   - Migrations will be applied on first startup
   - Monitor logs for initialization success

4. **Verify:**
   ```bash
   curl https://your-app.com/api/health/status
   curl https://your-app.com/api/health/database
   ```

### Backup Strategy

**SQL Server Backups:**
```sql
-- Full backup
BACKUP DATABASE [STOM_DB] 
TO DISK = N'C:\Backups\STOM_DB_Full.bak'
WITH INIT, COMPRESSION;

-- Transaction log backup (regular intervals)
BACKUP LOG [STOM_DB]
TO DISK = N'C:\Backups\STOM_DB_Log.trn'
WITH INIT;
```

---

## Performance Optimization

### Indexes

The `StomDbContext.OnModelCreating()` automatically creates:
- Descending index on `AuditLog.Timestamp`
- Index on `AuditLog.PerformedBy`

### Connection Pooling

Connection pooling is automatically enabled with recommended defaults.

### Query Store

Enabled in production to track query performance:
```sql
ALTER DATABASE [STOM_DB] SET QUERY_STORE = ON;
```

---

## Related Files

- **Setup Code:** `STOM.API/Program.cs`
- **DB Context:** `STOM.API/Data/StomDbContext.cs`
- **Initialization Service:** `STOM.API/Services/DatabaseInitializationService.cs`
- **Health Controller:** `STOM.API/Controllers/HealthController.cs`
- **SQL Reference:** `SQL_SERVER_SETUP.sql`
- **DB Setup Doc:** `DB_SETUP.md` (Offline-capable architecture)

---

## FAQ

**Q: Do I need to manually create the database?**
A: No. It's created automatically on first startup.

**Q: Can multiple instances share the database?**
A: Yes. Multiple STOM.API instances can safely connect to the same database.

**Q: How do I backup the database?**
A: Use SQL Server backup tools or the scripts in `SQL_SERVER_SETUP.sql`.

**Q: Can I use a different SQL Server version?**
A: Yes. Update the connection string to point to your SQL Server instance.

**Q: What if migrations fail?**
A: The application logs the error and halts startup to prevent data inconsistency.

**Q: How do I reset the database?**
A: Delete the database from SQL Server, then restart the application.

