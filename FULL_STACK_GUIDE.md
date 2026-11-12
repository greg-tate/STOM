# STOM - Full Stack Deployment Guide

## ✅ Project Status

| Project | Status | Port | Purpose |
|---------|--------|------|---------|
| **STOM.API** | Ready | 5022 | REST API Backend |
| **STOM.App** | Ready | 5049 | Blazor Web Client |
| **STOM.Database** | Reference | N/A | Database scripts |

---

## 🚀 Quick Start - Run Full Stack

### Step 1: Start the API Server

**Terminal 1:**
```bash
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

**Wait for message:**
```
Now listening on: http://localhost:5022
Now listening on: https://localhost:7022
```

**Verify database initialized:**
```
========== DATABASE INITIALIZATION SUCCESS ==========
```

### Step 2: Start the Blazor Web Client

**Terminal 2:**
```bash
cd C:\Source\Repos\STOM\STOM.App
dotnet run
```

**Wait for message:**
```
Now listening on: http://localhost:5049
Now listening on: https://localhost:7102
```

**Browser should open automatically** at `http://localhost:5049`

---

## 🧪 Testing

### Test API Endpoints (while running)

```bash
# Simple health check
curl http://localhost:5022/api/health/status

# Detailed database info
curl http://localhost:5022/api/health/database
```

**Expected responses:**
```json
{
  "status": "Healthy",
  "message": "All systems operational",
  "timestamp": "2024-11-13T00:00:00Z"
}
```

```json
{
  "canConnect": true,
  "appliedMigrationsCount": 1,
  "appliedMigrations": ["20261109133537_InitialCreate"],
  "pendingMigrationsCount": 0,
  "pendingMigrations": [],
  "timestamp": "2024-11-13T00:00:00Z"
}
```

### Test Blazor App

- **Browser:** `http://localhost:5049`
- App should load and connect to API at `http://localhost:5022`

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  STOM Application                       │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Internet Browser                                       │
│  (User's Device)                                        │
│         │                                               │
│         │ Port 5049                                     │
│         ▼                                               │
│  ┌──────────────────────────┐                           │
│  │  STOM.App                │                           │
│  │  (Blazor WebAssembly)    │                           │
│  │  - Pages & Components    │                           │
│  │  - IndexedDB Cache       │                           │
│  │  - Offline Support       │                           │
│  └──────────┬───────────────┘                           │
│             │                                           │
│             │ HTTP Port 5022                            │
│             ▼                                           │
│  ┌──────────────────────────┐                           │
│  │  STOM.API                │                           │
│  │  (ASP.NET Core)          │                           │
│  │  - REST Endpoints        │                           │
│  │  - Authentication        │                           │
│  │  - Business Logic        │                           │
│  └──────────┬───────────────┘                           │
│             │                                           │
│             │ TCP Port 1433                             │
│             ▼                                           │
│  ┌──────────────────────────┐                           │
│  │  SQL Server              │                           │
│  │  Database                │                           │
│  │  - STOM_DB_Dev           │                           │
│  │  - Tournaments           │                           │
│  │  - Players               │                           │
│  │  - Games & Scores        │                           │
│  └──────────────────────────┘                           │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 📋 Component Details

### STOM.API (Backend)

**What it provides:**
- REST API for tournament scoring
- User authentication and authorization
- Business logic for tournament management
- Database access and migrations
- Health check endpoints

**Key Endpoints:**
- `GET /api/health/status` - Simple health check
- `GET /api/health/database` - Database diagnostics
- `/openapi/v1.json` - API documentation (development only)

**Ports:**
- HTTP: `http://localhost:5022`
- HTTPS: `https://localhost:7022`

**Configuration:**
- `appsettings.json` - Production config
- `appsettings.Development.json` - Development config
- `launchSettings.json` - Port configuration

### STOM.App (Frontend)

**What it provides:**
- Responsive web interface for tournament scoring
- Blazor WebAssembly components
- Client-side routing and navigation
- IndexedDB for offline support
- Real-time updates

**Features:**
- Tournament management
- Player roster management
- Live scoring interface
- Audit log viewing
- Offline capability with automatic sync

**Ports:**
- HTTP: `http://localhost:5049`
- HTTPS: `https://localhost:7102`

**Configuration:**
- `Program.cs` - API connection: `http://localhost:5022`
- `launchSettings.json` - Port configuration

### SQL Server Database

**What it stores:**
- Tournament definitions and scheduling
- Player profiles and ratings
- Game results and scores
- Audit logs for compliance
- Migration history

**Databases:**
- `STOM_DB_Dev` - Development database (currently in use)
- `STOM_DB` - Production database

**Connection:**
- Server: `.` (local SQL Server)
- Port: `1433` (default)
- Authentication: Windows (Integrated Security)
- SSL: Trusted certificates required

---

## 🔧 Configuration Files

### API Configuration

**STOM.API/appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  },
  "DatabaseSettings": {
    "AutoMigrate": true,
    "EnsureCreated": false
  }
}
```

**STOM.API/Properties/launchSettings.json:**
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:5022",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### App Configuration

**STOM.App/Program.cs:**
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5022")  // API server address
});
```

**STOM.App/Properties/launchSettings.json:**
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:5049",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 🚨 Troubleshooting

### API Won't Start

**Problem:** "Address already in use"

**Solution:**
```powershell
# Find process using port 5022
Get-NetTCPConnection -LocalPort 5022 | Select-Object -ExpandProperty OwningProcess

# Kill the process
Stop-Process -Id <PID> -Force
```

### Blazor App Can't Connect to API

**Problem:** CORS error or connection refused

**Solution:**
1. Verify API is running on port 5022
2. Check if CORS is enabled (should be by default)
3. Verify connection string in `STOM.App/Program.cs`:
   ```csharp
   BaseAddress = new Uri("http://localhost:5022")
   ```

### Database Connection Error

**Problem:** "The certificate chain was issued by an authority that is not trusted"

**Solution:** Already fixed in `appsettings.Development.json`:
```json
"DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;TrustServerCertificate=true;"
```

### SSL/TLS Certificate Error

**Problem:** "The remote certificate is invalid"

**Solution:**
- Development: Self-signed certificates are normal, ignore warning
- Production: Use proper SSL certificates from trusted CA

---

## 📚 Documentation

### Getting Started
- **PORT_CONFIGURATION.md** - Port mapping and configuration
- **QUICK_REFERENCE.md** - Quick commands
- **SETUP_COMPLETE.md** - Setup summary

### Database
- **SQL_SERVER_SETUP_DEV_PROD.md** - Complete SQL Server setup
- **DATABASE_SETUP_GUIDE.md** - Database operations
- **DB_SETUP.md** - Architecture with offline capability

### Reference
- **SQL_SERVER_CONFIG_SUMMARY.md** - Configuration overview
- **CHANGES.md** - Technical changes made
- **FIX_SUMMARY.md** - Bug fixes applied

---

## ✅ Verification Checklist

### Before Running

- [ ] SQL Server is installed and running
- [ ] STOM_DB_Dev database exists or will be created
- [ ] Ports 5022, 5049 are available
- [ ] .NET 8+ runtime installed
- [ ] Projects build successfully: `dotnet build`

### After Starting API

- [ ] No startup errors in console
- [ ] "DATABASE INITIALIZATION SUCCESS" message shown
- [ ] API listening on `http://localhost:5022`
- [ ] Health check responds: `curl http://localhost:5022/api/health/status`

### After Starting App

- [ ] Browser opens to `http://localhost:5049`
- [ ] Blazor app loads without errors
- [ ] App connects to API (check browser console for errors)
- [ ] UI elements are responsive

---

## 🎯 What to Do Next

### 1. Run the Full Stack
```bash
# Terminal 1
cd STOM.API && dotnet run

# Terminal 2  
cd STOM.App && dotnet run
```

### 2. Test the Application
- Navigate to `http://localhost:5049`
- Create a tournament
- Add players
- Start scoring

### 3. Verify Database
```bash
# Check database created
sqlcmd -S . -Q "SELECT name FROM sys.databases WHERE name LIKE 'STOM%'"

# Check tables
sqlcmd -S . -d "STOM_DB_Dev" -Q "SELECT name FROM sys.tables"
```

### 4. Monitor Logs
- Watch API console for requests from Blazor app
- Check for any connection or business logic errors
- Verify audit logs are being recorded

---

## 🔐 Security Notes

### Development
- Self-signed SSL certificates are used
- Authentication is simplified for testing
- CORS allows any origin (development only)
- Debug logging enabled

### Production
- Use proper SSL certificates from trusted CA
- Implement proper authentication (JWT, OAuth)
- Restrict CORS to specific origins
- Disable debug logging
- Use secure passwords for SQL Server
- Set appropriate role-based access control

---

## 📞 Support

**Quick Questions:**
- See `QUICK_REFERENCE.md`

**Port Issues:**
- See `PORT_CONFIGURATION.md`

**Database Problems:**
- See `DATABASE_SETUP_GUIDE.md`

**Architecture Questions:**
- See `DB_SETUP.md`

---

## Summary

✅ **All components ready to run**
✅ **Ports configured correctly**
✅ **API connects to SQL Server**
✅ **Blazor app connects to API**
✅ **Database initialized automatically**

**Status: 🚀 Ready for full stack deployment!**

Start with Terminal 1 running STOM.API, then Terminal 2 running STOM.App.

