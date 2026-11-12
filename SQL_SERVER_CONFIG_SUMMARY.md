# SQL Server Configuration - Development & Production Setup

## Summary of Changes

STOM has been updated to use **SQL Server** (instead of LocalDB) for both development and production environments.

---

## Connection Strings Updated

### Development: appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;"
  }
}
```

### Production: appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB;Integrated Security=true;"
  }
}
```

---

## Architecture

```
STOM Deployment Model:

Development:
┌──────────────────┐
│   SQL Server     │  ← Central backend
│   STOM_DB_Dev    │  (Developer's local instance)
└────────┬─────────┘
         │
         ↓
   REST API
         ↓
┌──────────────────────┐
│  Blazor Web Client   │
├──────────────────────┤
│   IndexedDB Cache    │  ← Offline storage & sync
└──────────────────────┘

Production:
┌──────────────────┐
│   SQL Server     │  ← Enterprise backend
│   STOM_DB        │  (Production instance)
└────────┬─────────┘
         │
         ↓
   REST API
         ↓
┌──────────────────────┐
│  Blazor Web Client   │
├──────────────────────┤
│   IndexedDB Cache    │  ← Offline storage & sync
└──────────────────────┘
```

---

## Key Points

### ✅ What Changed
- Removed LocalDB connection strings
- Updated to SQL Server connection strings
- Connection strings use `.` (dot) for local SQL Server
- Both dev and prod now use real SQL Server instances

### ✅ IndexedDB Role
- **Offline Cache** - Caches tournament and player data
- **Sync Queue** - Stores pending score changes while offline
- **Conflict Resolution** - Handles sync when connection restored
- **Not a replacement** - Still syncs back to SQL Server

### ✅ Benefits
- **Development** - Uses isolated `STOM_DB_Dev` database
- **Production** - Uses `STOM_DB` database on production SQL Server
- **Consistency** - Same database technology for both environments
- **Scalability** - SQL Server supports multi-user access and high performance

---

## Getting Started

### Verify SQL Server Installation

```bash
# Test connection
sqlcmd -S . -Q "SELECT @@VERSION"

# If connection fails:
# 1. Install SQL Server Express or Developer Edition
# 2. Ensure SQL Server service is running
# 3. Enable Network Protocols (Named Pipes, TCP/IP)
```

### Run Application (Development)

```bash
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

**Expected output:**
```
========== DATABASE INITIALIZATION SUCCESS ==========
```

Database `STOM_DB_Dev` will be created automatically.

### Verify Database Created

```bash
# List databases
sqlcmd -S . -Q "SELECT name FROM sys.databases WHERE name LIKE 'STOM%'"

# List tables
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT name FROM sys.tables"
```

---

## Configuration by Environment

### Local Development
```
Server=.
Database=STOM_DB_Dev
Auth=Integrated Security (Windows)
Logging=Debug level
```

### Production (Local SQL Server)
```
Server=.
Database=STOM_DB
Auth=Integrated Security (Windows)
Logging=Information level
```

### Production (Remote SQL Server)
```
Server=your-sql-server.company.com
Database=STOM_DB
Auth=SQL Server login (User Id + Password)
Logging=Information level
Encryption=Required
```

---

## Documentation

### Primary References
- **SQL_SERVER_SETUP_DEV_PROD.md** - Complete SQL Server setup guide for dev and production
- **DATABASE_SETUP_GUIDE.md** - Database setup and operations guide
- **QUICK_REFERENCE.md** - Quick commands and connections

### Architecture References
- **DB_SETUP.md** - SQL Server + IndexedDB hybrid architecture
- **DB_IMPLEMENTATION_INDEX.md** - Complete documentation index

---

## Files Changed

✅ **appsettings.json**
- Changed from LocalDB to SQL Server connection
- Server: `(localdb)\mssqllocaldb` → `.`

✅ **appsettings.Development.json**
- Changed from LocalDB to SQL Server connection
- Server: `(localdb)\mssqllocaldb` → `.`
- Database: `STOM_DB_Dev` (unchanged, still isolated)

---

## Next Steps

1. **Verify SQL Server Installation**
   ```bash
   sqlcmd -S . -Q "SELECT @@VERSION"
   ```

2. **Run Application**
   ```bash
   cd STOM.API
   set ASPNETCORE_ENVIRONMENT=Development
   dotnet run
   ```

3. **Verify Database Creation**
   ```bash
   sqlcmd -S . -Q "SELECT name FROM sys.databases WHERE name LIKE 'STOM%'"
   ```

4. **Test API**
   ```bash
   curl http://localhost:5000/api/health/status
   ```

---

## Benefits of SQL Server (vs LocalDB)

| Feature | LocalDB | SQL Server |
|---------|---------|-----------|
| **Performance** | Good | Excellent |
| **Multi-user** | No | Yes ✅ |
| **Backup/Restore** | Complex | Built-in ✅ |
| **Security** | Basic | Advanced ✅ |
| **Production Ready** | No | Yes ✅ |
| **Licensing** | Free | Free (Express) or Licensed |
| **Storage Limit** | Unlimited | Express: 10GB |

---

## Connection String Examples

### Local Development (Windows Auth)
```
Server=.;Database=STOM_DB_Dev;Integrated Security=true;
```

### Local Development (Named Instance)
```
Server=.\SQLEXPRESS;Database=STOM_DB_Dev;Integrated Security=true;
```

### Remote Production (SQL Auth)
```
Server=your-sql-server.company.com;Database=STOM_DB;User Id=stom_user;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;
```

### Azure SQL Database
```
Server=your-server.database.windows.net;Database=STOM_DB;User Id=stom_user@your-server;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;
```

---

## Summary

✅ **SQL Server configured for both development and production**
✅ **Maintains isolated dev database (STOM_DB_Dev)**
✅ **IndexedDB provides offline capability and sync**
✅ **Ready for multi-user tournament scoring**
✅ **Scales to production deployments**

Build verified and ready to run! 🚀

