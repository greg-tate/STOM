# Quick Start Guide - STOM (Hybrid 3-Project Blazor Architecture)

## 📋 Prerequisites

- **.NET 11** (or latest version)
- **SQL Server LocalDB** (included with Visual Studio or SQL Server Express)
- **Two terminal windows** (or IDE terminals)
- Visual Studio 2022 / VS Code (optional)

## 🚀 Get Running in 5 Minutes

### Step 1: Verify .NET Installation
```powershell
dotnet --version
# Should show 11.0.x or higher
```

### Step 2: Setup Database (Run Once)
```powershell
cd C:\Source\Repos\STOM

# Create database and tables
sqlcmd -S "(localdb)\mssqllocaldb" -i STOM.Database\Scripts\001-CreateDatabase.sql
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB -i STOM.Database\Scripts\002-CreateTables.sql
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB -i STOM.Database\Scripts\003-SeedData.sql

echo "Database ready!"
```

### Step 3: Start STOM.API (Terminal 1)
```powershell
cd C:\Source\Repos\STOM\STOM.API
dotnet run
```

**Expected Output**:
```
Building...
Now listening on: http://localhost:5022
Application started. Press Ctrl+C to shut down.
```

### Step 4: Start STOM.App (Terminal 2)
```powershell
cd C:\Source\Repos\STOM\STOM.App
dotnet run
```

**Expected Output**:
```
Building...
Now listening on: http://localhost:5173
Application started. Press Ctrl+C to shut down.
```

### Step 5: Open in Browser
- Open: **http://localhost:5173**
- Choose a role: Scorer, Admin, or SuperAdmin
- Click "Go to [Role]" to see role-specific UI

## 🎯 What You'll See

### Scorer Role
- **URL**: http://localhost:5173/score/1
- **Features**: Enter scores for games
- **Future**: Offline support (IndexedDB sync)

### Admin Role
- **URL**: http://localhost:5173/admin/dashboard
- **Features**: View game standings, score averages
- **Future**: Approve scores, device monitoring

### SuperAdmin Role
- **URL**: http://localhost:5173/superadmin/console
- **Features**: Create tournaments, view audit logs
- **Future**: Player import, bracket generation

## 🔌 API Endpoints

### Available Now:
```
GET     http://localhost:5022/api/games
POST    http://localhost:5022/api/games/1/scores
GET     http://localhost:5022/api/admin/audit-logs
```

### Test with cURL:
```powershell
# Get all games
curl http://localhost:5022/api/games

# Add a score to game 1 (value 100)
curl -X POST http://localhost:5022/api/games/1/scores `
  -H "Content-Type: application/json" `
  -d '{"value": 100}'
```

## 📁 Project Structure

```
STOM/
├── STOM.API/              ← REST API (port 5022)
│   ├── Controllers/       (GamesController, AdminController)
│   ├── Models/            (Game, Score, Tournament, etc.)
│   ├── Services/          (AuditService, etc.)
│   └── Program.cs         (API configuration)
│
├── STOM.App/              ← Blazor WASM App (port 5173)
│   ├── Pages/             (Index, Scorer, Admin, SuperAdmin)
│   ├── Services/          (ApiService, AuthService, etc.)
│   └── Program.cs         (Blazor configuration)
│
├── STOM.Database/         ← SQL Scripts
│   ├── Scripts/           (Create DB, tables, seed data)
│   └── Tables/            (Individual table definitions)
│
└── STOM.slnx              ← Modern solution file
```

## 🐛 Troubleshooting

### "Cannot connect to database"
```powershell
# Check LocalDB status
sqllocaldb info mssqllocaldb

# If not running, start it
sqllocaldb start mssqllocaldb

# Recreate database
sqlcmd -S "(localdb)\mssqllocaldb" -i STOM.Database\Scripts\001-CreateDatabase.sql
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB -i STOM.Database\Scripts\002-CreateTables.sql
```

### "Port 5022 already in use"
```powershell
# Find and kill process
$proc = Get-Process | Where-Object {$_.Name -eq "dotnet"}
$proc | Stop-Process -Force

# Or change port in: STOM.API\Properties\launchSettings.json
```

### "Blazor app won't load"
```powershell
# Clear browser cache
# Ctrl+Shift+Delete → select "All time" → Clear all

# Then restart STOM.App and refresh browser
```

### API not responding
```powershell
# Verify API is running on port 5022
netstat -ano | findstr :5022

# If not listed, API is not running in Terminal 1
```

## 📚 Next Steps

1. **Explore the code**:
   - Check `STOM.API/Controllers/` for API endpoints
   - Check `STOM.App/Pages/` for Blazor components
   - Check `STOM.API/Services/` for business logic

2. **Add sample data**:
   - SuperAdmin: Create a new competition
   - Admin: View it on the dashboard
   - Scorer: Enter scores

3. **Enable features**:
   - Offline sync for Scorer role
   - Bracket generation for tournaments
   - Player bulk import

4. **Deploy**:
   - STOM.API → Azure App Service / AWS EC2
   - STOM.App → Azure Static Web Apps / GitHub Pages
   - STOM.Database → Azure SQL Database

## 📖 Documentation

- `README.md` - Full architecture guide
- `REWRITE_BLAZOR_WASM.md` - Original specification
- `REFACTORING_SUMMARY.md` - What was refactored

## 💡 Key Concepts

| Term | Meaning |
|------|---------|
| **PWA** | Progressive Web App (installable) |
| **WASM** | WebAssembly (runs in browser) |
| **LocalDB** | Local SQL Server instance |
| **AuditLog** | Immutable record of admin actions |
| **Role-Based Auth** | Different UIs for Scorer/Admin/SuperAdmin |

## ⚡ Commands Cheat Sheet

```powershell
# Start development
cd C:\Source\Repos\STOM\STOM.API && dotnet run    # Terminal 1
cd C:\Source\Repos\STOM\STOM.App && dotnet run    # Terminal 2

# Build solution
cd C:\Source\Repos\STOM && dotnet build

# Restore packages
cd C:\Source\Repos\STOM && dotnet restore

# Access the app
# Browser: http://localhost:5173
# API: http://localhost:5022

# View database
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB
> SELECT * FROM AuditLogs;
> GO
```

## 🎓 Learning Resources

- [Blazor WebAssembly Docs](https://docs.microsoft.com/aspnet/core/blazor/webassembly)
- [ASP.NET Core REST API Design](https://docs.microsoft.com/azure/architecture/best-practices/api-design)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)

---

**Happy coding!** 🚀

If you get stuck, check:
1. README.md for detailed documentation
2. REFACTORING_SUMMARY.md for architecture overview
3. Terminal output for error messages
