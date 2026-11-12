# SQL Server Setup Guide - Development & Production

## Overview

STOM uses **SQL Server** for both development and production environments. IndexedDB (browser-based) is used for offline caching and syncing back to SQL Server.

```
STOM Architecture:

Production/Development:
┌─────────────────────────┐
│     SQL Server          │  ← Central Backend (Source of Truth)
│     STOM_DB or          │
│     STOM_DB_Dev         │
└────────────┬────────────┘
             │
        REST API
             │
┌────────────▼────────────┐
│   Blazor Web Client     │
├─────────────────────────┤
│     IndexedDB Cache     │  ← Offline Data & Sync Queue
│  (Browser Storage)      │
└─────────────────────────┘
```

---

## SQL Server Installation

### Prerequisites

You need SQL Server 2019 or later (Express, Standard, or Developer Edition):

**Option 1: SQL Server Express (Free)**
- Download: https://www.microsoft.com/en-us/sql-server/sql-server-editions-express
- No license required
- Good for development and small deployments
- 10GB database size limit

**Option 2: SQL Server Developer Edition (Free)**
- Download: https://www.microsoft.com/en-us/sql-server/sql-server-editions-developer
- Full features, free for development only
- Recommended for STOM development

**Option 3: SQL Server Standard/Enterprise**
- For production deployments
- Paid licenses required

### Installation Steps

1. **Download** SQL Server Express or Developer Edition
2. **Run Installer** and select "Basic" installation
3. **Accept Defaults** for most options
4. **Choose Authentication:**
   - ✅ Mixed Mode (SQL Server + Windows Authentication)
   - ✅ Windows Authentication only (Integrated Security)
5. **Note the Server Name** (usually `.\SQLEXPRESS` or `.` for default)
6. **Verify Installation:**
   ```bash
   sqlcmd -S . -Q "SELECT @@VERSION"
   ```

---

## Connection Strings

### Development Configuration

**File:** `STOM.API/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;"
  }
}
```

**Connection Details:**
- **Server:** `.` (dot = local SQL Server instance)
- **Database:** `STOM_DB_Dev` (development database)
- **Authentication:** `Integrated Security=true` (Windows Authentication)

**Alternative for Named Instance:**
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=STOM_DB_Dev;Integrated Security=true;"
```

### Production Configuration

**File:** `STOM.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB;Integrated Security=true;"
  }
}
```

**For Remote SQL Server:**
```json
"DefaultConnection": "Server=your-sql-server.company.com;Database=STOM_DB;User Id=stom_user;Password=YourSecurePassword;Encrypt=true;TrustServerCertificate=false;"
```

**Connection Details:**
- **Server:** SQL Server hostname or IP
- **Database:** `STOM_DB` (production database)
- **User Id/Password:** SQL Server login credentials
- **Encrypt:** Use encrypted connections for production
- **TrustServerCertificate:** `false` for production (validates certificates)

---

## First-Time Setup

### 1. Verify SQL Server is Running

```bash
# Check if SQL Server is running
sqlcmd -S . -Q "SELECT @@VERSION"

# If connection fails, verify:
# - SQL Server service is running (Services.msc)
# - Network protocols enabled (SQL Server Configuration Manager)
# - Firewall allows port 1433 (for remote connections)
```

### 2. Run STOM.API Application

```bash
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### 3. Watch for Initialization Log

```
========== DATABASE INITIALIZATION START ==========
Using connection string: Server=.;Database=STOM_DB_Dev;Integrated Security=true;
Found 1 pending migrations:
  - 20261109133537_InitialCreate
Applying pending migrations...
All pending migrations applied successfully.
========== DATABASE INITIALIZATION SUCCESS ==========
```

### 4. Verify Database Creation

```bash
# List databases
sqlcmd -S . -Q "SELECT name FROM sys.databases WHERE name LIKE 'STOM%'"

# Output should show:
# name
# ----
# STOM_DB_Dev
```

### 5. Verify Tables Created

```bash
# List tables
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT name FROM sys.tables"

# Output should show all 6 tables:
# - AuditLogs
# - Competitions
# - Games
# - Players
# - Scores
# - Tournaments
```

---

## Development Workflow

### Start Development

```bash
# Terminal 1: Run API Server
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Terminal 2: Run Blazor Client (if needed)
cd C:\Source\Repos\STOM\STOM.App
dotnet run
```

### Database Modifications

**Add New Table/Fields:**

1. Create model in `STOM.API/Models/`
   ```csharp
   public class NewEntity
   {
       public int Id { get; set; }
       public string Name { get; set; }
   }
   ```

2. Add DbSet to `STOM.API/Data/StomDbContext.cs`
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

3. Create migration
   ```bash
   cd STOM.API
   dotnet ef migrations add AddNewEntity
   ```

4. Run application (migration applies automatically)
   ```bash
   dotnet run
   ```

### View Database in SQL Server Management Studio

**Connect:**
- **Server Name:** `.` (or `.\SQLEXPRESS`)
- **Authentication:** Windows Authentication
- **Database:** `STOM_DB_Dev` or `STOM_DB`

**Query Example:**
```sql
-- View all players
SELECT * FROM Players;

-- View recent audit logs
SELECT TOP 100 * FROM AuditLogs ORDER BY Timestamp DESC;

-- Check applied migrations
SELECT * FROM __EFMigrationsHistory;
```

---

## Production Deployment

### Pre-Deployment

1. **SQL Server Setup**
   ```bash
   # On production server
   sqlcmd -S your-server -Q "SELECT @@VERSION"
   ```

2. **Create Database User** (if using SQL authentication)
   ```sql
   -- On production SQL Server
   CREATE LOGIN stom_user WITH PASSWORD = 'YourSecurePassword123!';
   CREATE DATABASE STOM_DB;
   CREATE USER stom_user FOR LOGIN stom_user;
   ALTER ROLE db_owner ADD MEMBER stom_user;
   ```

3. **Update Connection String**
   ```json
   // STOM.API/appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your-sql-server.company.com;Database=STOM_DB;User Id=stom_user;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;"
     }
   }
   ```

4. **Verify Connectivity**
   ```bash
   sqlcmd -S your-sql-server.company.com -U stom_user -P YourSecurePassword123! -Q "SELECT @@VERSION"
   ```

### Deploy Application

```bash
# Build release version
cd C:\Source\Repos\STOM
dotnet publish -c Release -o ./publish

# Copy to production server
# (Via RDP, SSH, Azure Deploy, etc.)

# On production server, run:
cd publish
dotnet STOM.API.dll

# Database will be created and migrated automatically
```

### Verify Production Setup

```bash
# Check API is running
curl https://your-stom-app.com/api/health/status

# Response should be:
# {
#   "status": "Healthy",
#   "message": "All systems operational",
#   "timestamp": "2024-11-12T23:30:00Z"
# }

# Check database status
curl https://your-stom-app.com/api/health/database

# Response should show applied migrations
```

---

## Backup & Recovery

### Backup Development Database

```bash
# Full backup
sqlcmd -S . -Q "BACKUP DATABASE [STOM_DB_Dev] TO DISK = 'C:\Backups\STOM_DB_Dev.bak' WITH INIT, COMPRESSION;"

# Verify backup
sqlcmd -S . -Q "RESTORE VERIFYONLY FROM DISK = 'C:\Backups\STOM_DB_Dev.bak';"
```

### Backup Production Database

**Automated Backup (Recommended):**
```sql
-- SQL Server Agent Job
BACKUP DATABASE [STOM_DB] 
TO DISK = N'\\backup-server\sql-backups\STOM_DB_$(ESCAPE_SQUOTE(DATE)).bak'
WITH INIT, COMPRESSION, STATS = 10;
```

**Manual Backup:**
```bash
sqlcmd -S your-sql-server -U stom_user -P YourPassword -Q "BACKUP DATABASE [STOM_DB] TO DISK = 'C:\Backups\STOM_DB.bak' WITH INIT, COMPRESSION;"
```

### Restore Database

```sql
-- Restore from backup
RESTORE DATABASE [STOM_DB] 
FROM DISK = 'C:\Backups\STOM_DB.bak'
WITH REPLACE;
```

---

## Troubleshooting

### Issue: "Cannot connect to Server ."

**Solutions:**
```bash
# Verify SQL Server is running
Get-Service -Name MSSQL$SQLEXPRESS

# Start if stopped
Start-Service -Name MSSQL$SQLEXPRESS

# Check SQL Server Configuration Manager
# - Enable Named Pipes
# - Enable TCP/IP
```

### Issue: "Login failed for user"

**For Windows Authentication:**
- Ensure your Windows user is in the SQL Server logins
- Run SQL Server Management Studio as Administrator

**For SQL Server Authentication:**
- Verify username and password are correct
- Check connection string for typos

### Issue: "Database already exists" error

**Solution:**
```bash
# Delete the database
sqlcmd -S . -Q "DROP DATABASE STOM_DB_Dev;"

# Run application again (recreates and migrates)
dotnet run
```

### Issue: "Migrations not applying"

**Solution:**
```bash
# Check pending migrations
cd STOM.API
dotnet ef migrations list

# Apply manually
dotnet ef database update

# Check applied migrations
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT * FROM __EFMigrationsHistory;"
```

---

## Environment Variables

### For Development
```bash
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=https://localhost:7001;http://localhost:5001
dotnet run
```

### For Production
```bash
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://0.0.0.0:443
dotnet run
```

---

## Connection String Reference

### Development (Local SQL Server)
```
Server=.;Database=STOM_DB_Dev;Integrated Security=true;
```

### Production (Named Instance)
```
Server=.\SQLEXPRESS;Database=STOM_DB;Integrated Security=true;
```

### Production (Remote Server, Windows Auth)
```
Server=your-sql-server.company.com;Database=STOM_DB;Integrated Security=true;Encrypt=true;
```

### Production (Remote Server, SQL Auth)
```
Server=your-sql-server.company.com;Database=STOM_DB;User Id=stom_user;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;
```

### Production (Azure SQL)
```
Server=your-server.database.windows.net;Database=STOM_DB;User Id=stom_user@your-server;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;
```

---

## Performance Optimization

### Indexes

Automatically created by Entity Framework:
- `AuditLog.Timestamp` (descending) - Fast audit log queries
- `AuditLog.PerformedBy` - Quick user activity lookups

### Connection Pooling

Automatically enabled with recommended defaults:
- Max Pool Size: 100
- Min Pool Size: 5
- Connection Timeout: 30 seconds

### Query Store

Enable for production to track query performance:
```sql
ALTER DATABASE [STOM_DB] SET QUERY_STORE = ON;
```

---

## Support & Documentation

- **Health Endpoints:** `/api/health/status`, `/api/health/database`
- **Setup Guide:** `DATABASE_SETUP_GUIDE.md`
- **Architecture:** `DB_SETUP.md`
- **Quick Reference:** `QUICK_REFERENCE.md`

