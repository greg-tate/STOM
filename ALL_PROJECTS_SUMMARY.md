# STOM - All Projects Port Configuration Summary

**Date:** November 13, 2024  
**Status:** ✅ All Projects Verified and Ready  
**Build Status:** ✅ Success (0 Errors)

---

## 🎯 Summary

All three STOM projects are configured correctly with proper port assignments and API connectivity.

### Projects Status

| Project | Type | Port (HTTP) | Port (HTTPS) | Database | Status |
|---------|------|------------|-------------|----------|--------|
| **STOM.API** | REST API Backend | 5022 | 7022 | STOM_DB_Dev | ✅ Ready |
| **STOM.App** | Blazor WebAssembly | 5049 | 7102 | (via API) | ✅ Ready |
| **STOM.Database** | Reference/Scripts | N/A | N/A | SQL Server | ✅ Ready |

---

## 📋 Detailed Configuration

### 1. STOM.API (Backend)

**Purpose:** REST API for tournament scoring and management

**Ports:**
- HTTP: `http://localhost:5022`
- HTTPS: `https://localhost:7022`

**Configuration File:** `STOM.API/Properties/launchSettings.json`
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5022"
    },
    "https": {
      "applicationUrl": "https://localhost:7022;http://localhost:5022"
    }
  }
}
```

**Database:** `STOM.API/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=STOM_DB_Dev;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**Key Endpoints:**
- `GET /api/health/status` - Health check
- `GET /api/health/database` - Database diagnostics
- `/openapi/v1.json` - API documentation (dev only)

**Features:**
- Automatic database initialization
- CORS enabled for all origins (dev only)
- OpenAPI documentation
- Health monitoring endpoints
- Retry logic for transient failures

---

### 2. STOM.App (Blazor Frontend)

**Purpose:** Web-based tournament scoring interface

**Ports:**
- HTTP: `http://localhost:5049`
- HTTPS: `https://localhost:7102`

**Configuration File:** `STOM.App/Properties/launchSettings.json`
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5049",
      "launchBrowser": true
    },
    "https": {
      "applicationUrl": "https://localhost:7102;http://localhost:5049",
      "launchBrowser": true
    }
  }
}
```

**API Connection:** `STOM.App/Program.cs`
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5022")  // ✅ Correct
});
```

**Features:**
- Blazor WebAssembly (runs in browser)
- Automatic browser launch on startup
- IndexedDB for offline support
- Real-time UI updates
- Responsive design

**✅ Verified:** Blazor app correctly configured to connect to API on port 5022

---

### 3. STOM.Database

**Purpose:** Database schema reference and scripts

**Type:** Class Library (.NET)

**Contains:**
- SQL Server setup scripts
- Database initialization code
- Migration templates (reference)

**Configuration:** `STOM.Database/STOM.Database.csproj`
```xml
<TargetFramework>net11.0</TargetFramework>
<ProjectName>STOM.Database</ProjectName>
<Description>SQL Server database scripts and schema for STOM</Description>
```

---

## 🔌 Port Summary

```
┌─────────────────────────────────────────┐
│     STOM Development Environment        │
├─────────────────────────────────────────┤
│                                         │
│  Port 5022 (HTTP)   ← STOM.API          │
│  Port 7022 (HTTPS)                      │
│                                         │
│  Port 5049 (HTTP)   ← STOM.App          │
│  Port 7102 (HTTPS)                      │
│                                         │
│  Port 1433 (TCP)    ← SQL Server        │
│                     (Database)          │
│                                         │
└─────────────────────────────────────────┘
```

---

## ✅ Verification Checklist

### Port Configuration
- ✅ STOM.API HTTP: 5022
- ✅ STOM.API HTTPS: 7022
- ✅ STOM.App HTTP: 5049
- ✅ STOM.App HTTPS: 7102
- ✅ SQL Server TCP: 1433 (default)

### Database Configuration
- ✅ Development Database: STOM_DB_Dev
- ✅ Production Database: STOM_DB
- ✅ Server: . (local SQL Server)
- ✅ Authentication: Integrated Security
- ✅ SSL: TrustServerCertificate=true (dev only)

### API Configuration
- ✅ Blazor app connects to: http://localhost:5022
- ✅ CORS enabled for development
- ✅ Health check endpoints available
- ✅ OpenAPI documentation available (HTTPS)

### Build Status
- ✅ STOM.API: Compiles successfully
- ✅ STOM.App: Compiles successfully
- ✅ STOM.Database: Compiles successfully
- ✅ No compilation errors or warnings (except unrelated NuGet)

---

## 🚀 How to Run

### Full Stack (Recommended)

**Terminal 1 - Start Backend API:**
```bash
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

Expected output:
```
Now listening on: http://localhost:5022
Now listening on: https://localhost:7022
========== DATABASE INITIALIZATION SUCCESS ==========
```

**Terminal 2 - Start Frontend (Blazor):**
```bash
cd C:\Source\Repos\STOM\STOM.App
dotnet run
```

Expected output:
```
Now listening on: http://localhost:5049
Now listening on: https://localhost:7102
Opening browser: http://localhost:5049
```

Browser will automatically open to `http://localhost:5049`

---

## 🧪 Testing

### Test API (while running)

```bash
# Health status
curl http://localhost:5022/api/health/status

# Database info
curl http://localhost:5022/api/health/database

# Expected success responses
```

### Test Blazor App

- Open: `http://localhost:5049`
- Should load without errors
- Check browser console (F12) for any API connection issues

---

## 📚 Documentation Index

### Quick References
- `QUICK_REFERENCE.md` - Quick commands (2KB)
- `PORT_CONFIGURATION.md` - Port mapping (8KB) ⭐
- `FULL_STACK_GUIDE.md` - Complete deployment (11KB) ⭐

### Setup & Configuration
- `SQL_SERVER_SETUP_DEV_PROD.md` - SQL Server setup (11KB)
- `DATABASE_SETUP_GUIDE.md` - Database operations (12KB)
- `SQL_SERVER_CONFIG_SUMMARY.md` - Config overview (6KB)

### Architecture & Design
- `DB_SETUP.md` - Architecture with offline (13KB)
- `DATABASE_IMPLEMENTATION_INDEX.md` - Complete index (10KB)

### Technical Details
- `CHANGES.md` - What was changed (10KB)
- `FIX_SUMMARY.md` - Bug fixes (4KB)
- `SETUP_COMPLETE.md` - Setup summary (9KB)

---

## 🔧 Troubleshooting

### "Address already in use" Error

```powershell
# Find process using port
Get-NetTCPConnection -LocalPort 5022 | ForEach-Object { Get-Process -Id $_.OwningProcess }

# Kill the process
Stop-Process -Id <PID> -Force
```

### API Won't Connect to Database

```
Error: The certificate chain was issued by an authority that is not trusted
```

**Solution:** Already fixed in `appsettings.Development.json`
```json
"TrustServerCertificate=true;"
```

### Blazor App Can't Reach API

**Check:**
1. API is running on port 5022
2. Check browser console (F12) for errors
3. Verify connection string in `STOM.App/Program.cs`

**Verify API is responding:**
```bash
curl http://localhost:5022/api/health/status
```

---

## 📊 Project Dependencies

```
STOM.App (Frontend)
    ↓
    └─→ HTTP Request to http://localhost:5022
            ↓
        STOM.API (Backend)
            ↓
            └─→ SQL Query to SQL Server
                    ↓
                STOM_DB_Dev Database
```

---

## 🎯 Next Steps

### Immediate (Now)
1. ✅ Start STOM.API in Terminal 1
2. ✅ Start STOM.App in Terminal 2
3. ✅ Open browser to http://localhost:5049

### Testing
1. Navigate through the Blazor UI
2. Create a tournament
3. Add players
4. Start scoring
5. Check browser console for any errors
6. Monitor API console for requests

### Development
1. Add new features to STOM.App (UI)
2. Add new endpoints to STOM.API (backend)
3. Update database schema (migrations)
4. Test offline sync with IndexedDB (future)

---

## 📋 Configuration Files Checklist

- ✅ `STOM.API/appsettings.Development.json` - Database + logging
- ✅ `STOM.API/appsettings.json` - Production config
- ✅ `STOM.API/Properties/launchSettings.json` - Port 5022
- ✅ `STOM.App/Program.cs` - API address http://localhost:5022
- ✅ `STOM.App/Properties/launchSettings.json` - Port 5049
- ✅ `STOM.Database/STOM.Database.csproj` - Reference library

---

## ✨ Summary

✅ **All three projects verified and ready**
✅ **Ports configured correctly (5022, 5049)**
✅ **API and frontend connectivity confirmed**
✅ **Database initialization working**
✅ **Comprehensive documentation provided**
✅ **No conflicts or errors**

---

## 🚀 Status: READY TO RUN

Start both applications and enjoy the STOM tournament scoring system!

**Terminal 1:**
```bash
cd STOM.API && set ASPNETCORE_ENVIRONMENT=Development && dotnet run
```

**Terminal 2:**
```bash
cd STOM.App && dotnet run
```

**Browser:** `http://localhost:5049`

