# STOM Database Setup Guide - SQL Server Edition

## Overview

STOM uses **SQL Server** as its central backend database for both development and production environments. IndexedDB (browser-based storage) provides offline capability and syncs back to SQL Server.

---

## Architecture

```
Typical STOM Deployment:

┌─────────────────────────────┐
│      SQL Server             │  ← Central Backend
│   (STOM_DB or STOM_DB_Dev)  │  (Source of Truth)
│                             │
│  - Tournaments              │
│  - Players                  │
│  - Games & Scores           │
│  - Audit Logs               │
└────────────┬────────────────┘
             │
        REST API
             │
┌────────────▼────────────────┐
│  Blazor Web Application     │
├─────────────────────────────┤
│    Browser IndexedDB        │  ← Offline Cache
│                             │
│  - Cached tournament data   │
│  - Pending score changes    │
│  - Sync queue               │
└─────────────────────────────┘
```

---

## Prerequisites

### Required
- ✅ SQL Server 2019 or later (Express, Developer, or Standard Edition)
- ✅ .NET 8+ runtime
- ✅ Administrator access to SQL Server

### Installation
If SQL Server is not installed, download one of these:
- **SQL Server Express** (Free, 10GB limit): https://www.microsoft.com/en-us/sql-server/sql-server-editions-express
- **SQL Server Developer Edition** (Free, full features): https://www.microsoft.com/en-us/sql-server/sql-server-editions-developer

---

## Quick Start

### 1. Verify SQL Server Installation

```bash
# Test connection to local SQL Server
sqlcmd -S . -Q "SELECT @@VERSION"

# If this fails, SQL Server is not installed or not running
```

### 2. Update Connection String (if needed)

**For default SQL Server installation:**

File: `STOM.API/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;"
  }
}
```

**For named instance (e.g., SQLEXPRESS):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=STOM_DB_Dev;Integrated Security=true;"
  }
}
```

### 3. Run the Application

```bash
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### 4. Verify Database Creation

Watch for this message in the console:
```
========== DATABASE INITIALIZATION SUCCESS ==========
```

The database `STOM_DB_Dev` will be created automatically with all required tables.

---

## Database Configuration

### Development Database

**File:** `STOM.API/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

| Component | Value | Purpose |
|-----------|-------|---------|
| **Server** | `.` | Local SQL Server (default instance) |
| **Database** | `STOM_DB_Dev` | Development database (isolated) |
| **Authentication** | Windows | Uses your Windows user account |
| **Logging** | Debug | Verbose logging for troubleshooting |

### Production Database

**File:** `STOM.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB;Integrated Security=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

For remote SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-sql-server.company.com;Database=STOM_DB;User Id=stom_user;Password=YourSecurePassword;Encrypt=true;TrustServerCertificate=false;"
  }
}
```

---

## Automatic Database Initialization

When STOM.API starts, it automatically:

1. **Connects** to SQL Server using the connection string
2. **Creates database** if it doesn't exist
3. **Applies migrations** to create/update tables
4. **Logs progress** for debugging
5. **Reports success** or errors

### Initialization Log Example

```
========== DATABASE INITIALIZATION START ==========
Using connection string: Server=.;Database=STOM_DB_Dev;Integrated Security=true;
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

Location: `STOM.API/Migrations/`

| Migration | Name | Creates |
|-----------|------|---------|
| `20261109133537_InitialCreate.cs` | Initial Create | All core tables: Games, Scores, Tournaments, Competitions, Players, AuditLogs |

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
  "timestamp": "2024-11-12T23:30:00Z"
}
```

**Using SQL:**
```bash
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT * FROM __EFMigrationsHistory"
```

**Using CLI:**
```bash
cd STOM.API
dotnet ef migrations list
```

---

## Adding Database Changes

### Step 1: Modify the Model

Edit or create model in `STOM.API/Models/`

```csharp
public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    // Add new field
    public string Location { get; set; }
}
```

### Step 2: Update DbContext

Add or update DbSet in `STOM.API/Data/StomDbContext.cs`

```csharp
public DbSet<Tournament> Tournaments { get; set; }
```

### Step 3: Create Migration

```bash
cd STOM.API
dotnet ef migrations add AddTournamentLocation
```

This generates a new migration file: `Migrations/[Timestamp]_AddTournamentLocation.cs`

### Step 4: Apply Migration

**Option A:** Run application (automatic)
```bash
dotnet run
# Migration applies on startup
```

**Option B:** Manual update
```bash
dotnet ef database update
```

---

## Viewing Data in SQL Server

### Using SQL Server Management Studio

1. **Download & Install:** SQL Server Management Studio (Free)
2. **Connect:**
   - Server: `.` or `.\SQLEXPRESS`
   - Authentication: Windows Authentication
3. **Browse databases** → `STOM_DB_Dev` or `STOM_DB`
4. **Query examples:**

```sql
-- View all tournaments
SELECT * FROM Tournaments;

-- View all players
SELECT * FROM Players;

-- View recent audit logs
SELECT TOP 100 * FROM AuditLogs ORDER BY Timestamp DESC;

-- Count games
SELECT COUNT(*) as GameCount FROM Games;

-- View pending migrations
SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory;
```

### Using SQL Command Line

```bash
# Run query
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT name FROM sys.tables"

# Output:
# name
# AuditLogs
# Competitions
# Games
# Players
# Scores
# Tournaments
```

---

## Health Check Endpoints

### Simple Status Check

```bash
curl http://localhost:5000/api/health/status
```

**Response (Success - 200):**
```json
{
  "status": "Healthy",
  "message": "All systems operational",
  "timestamp": "2024-11-12T23:30:00Z"
}
```

**Response (Failure - 503):**
```json
{
  "status": "Unhealthy",
  "message": "Database connection failed",
  "timestamp": "2024-11-12T23:30:00Z"
}
```

### Detailed Database Info

```bash
curl http://localhost:5000/api/health/database
```

**Response:**
```json
{
  "canConnect": true,
  "appliedMigrationsCount": 1,
  "appliedMigrations": [
    "20261109133537_InitialCreate"
  ],
  "pendingMigrationsCount": 0,
  "pendingMigrations": [],
  "timestamp": "2024-11-12T23:30:00Z"
}
```

---

## Troubleshooting

### Issue: "Cannot connect to Server ."

**Cause:** SQL Server is not running or not installed

**Solution:**
```bash
# Check if running
Get-Service -Name MSSQL$SQLEXPRESS

# Start if stopped
Start-Service -Name MSSQL$SQLEXPRESS

# Verify connection
sqlcmd -S . -Q "SELECT @@VERSION"
```

### Issue: "Named Pipes Provider: Could not open connection"

**Cause:** Network protocols disabled

**Solution:**
1. Open SQL Server Configuration Manager
2. SQL Server Network Configuration → Protocols for SQLEXPRESS
3. Enable both **Named Pipes** and **TCP/IP**
4. Restart SQL Server service

### Issue: "Database 'STOM_DB_Dev' already exists"

**Cause:** Migration applying to existing database

**Solution:**
```bash
# Option 1: Let it continue (safe)
# The migration will recognize existing schema

# Option 2: Reset database
sqlcmd -S . -Q "DROP DATABASE STOM_DB_Dev"
# Then run application again
```

### Issue: "Login failed for user"

**Cause:** SQL Server authentication issue

**Solution:**
```bash
# For Windows Authentication:
# - Run as Administrator
# - Verify your Windows account is in SQL Server logins

# For SQL Authentication:
# - Verify username/password in connection string
# - Check user exists in SQL Server:
sqlcmd -S . -U sa -P YourPassword -Q "SELECT name FROM sys.sysusers"
```

### Issue: "Migration not applying"

**Cause:** Pending migrations not executing

**Solution:**
```bash
# Check pending migrations
cd STOM.API
dotnet ef migrations list

# Apply manually
dotnet ef database update

# Verify applied
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT * FROM __EFMigrationsHistory"
```

---

## Production Deployment

### Pre-Deployment Checklist

- ✅ SQL Server 2019+ installed on production server
- ✅ Database backup strategy in place
- ✅ Connection string verified and secure
- ✅ SQL login created with appropriate permissions
- ✅ Network connectivity tested

### Deployment Steps

1. **Update Connection String**
   ```json
   // STOM.API/appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=prod-sql.company.com;Database=STOM_DB;User Id=stom_user;Password=YourSecurePassword;Encrypt=true;"
     }
   }
   ```

2. **Build Release**
   ```bash
   cd C:\Source\Repos\STOM
   dotnet publish -c Release -o ./publish
   ```

3. **Deploy to Server**
   - Copy `publish` folder to production server
   - Set appropriate permissions

4. **Run Application**
   ```bash
   cd publish
   dotnet STOM.API.dll
   ```

5. **Verify**
   ```bash
   # Test health endpoint
   curl https://your-stom-app.com/api/health/status
   
   # Should return 200 with "Healthy" status
   ```

---

## Backup Strategy

### Development Backup

```bash
# Backup development database
sqlcmd -S . -Q "BACKUP DATABASE [STOM_DB_Dev] TO DISK = 'C:\Backups\STOM_DB_Dev.bak' WITH INIT, COMPRESSION;"

# Restore
sqlcmd -S . -Q "RESTORE DATABASE [STOM_DB_Dev] FROM DISK = 'C:\Backups\STOM_DB_Dev.bak' WITH REPLACE;"
```

### Production Backup (Recommended)

**Automated SQL Agent Job:**
```sql
-- Backup every night at 2 AM
CREATE JOB backup_stom_db
...
BACKUP DATABASE [STOM_DB] 
TO DISK = '\\backup-server\sql\STOM_DB_$(DATE).bak'
WITH INIT, COMPRESSION, STATS = 10;
```

**Manual Backup:**
```bash
sqlcmd -S your-sql-server -U stom_user -P YourPassword -Q "BACKUP DATABASE [STOM_DB] TO DISK = '\\backup-server\STOM_DB.bak' WITH INIT, COMPRESSION;"
```

---

## Performance Tips

### Indexes

Already created by Entity Framework:
- `AuditLogs.Timestamp` (descending)
- `AuditLogs.PerformedBy`

### Connection Pooling

Automatically managed with optimal settings:
- Max Pool Size: 100
- Connection Timeout: 30 seconds

### Query Optimization

Enable Query Store for production:
```sql
ALTER DATABASE [STOM_DB] SET QUERY_STORE = ON;
ALTER DATABASE [STOM_DB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE);
```

---

## Support & Related Documentation

- **SQL Server Setup (Dev + Prod):** `SQL_SERVER_SETUP_DEV_PROD.md` ← Full reference
- **Quick Reference:** `QUICK_REFERENCE.md`
- **Database Architecture:** `DB_SETUP.md`
- **Offline Sync Strategy:** `DB_SETUP.md`

