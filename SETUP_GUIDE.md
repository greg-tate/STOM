# STOM Setup Guide

## 🚀 Quick Start (5 Minutes)

### Prerequisites

- **.NET 11** or higher
- **SQL Server LocalDB** (included with Visual Studio)
- Two terminal windows

### Step 1: Setup Database (Run Once)

```powershell
cd C:\Source\Repos\STOM
# Create database (auto-migrations on startup)
# No manual scripts needed
```

### Step 2: Start STOM.API (Terminal 1)

```powershell
cd C:\Source\Repos\STOM\STOM.API
dotnet run
# → Listening on http://localhost:5022
```

### Step 3: Start STOM.App (Terminal 2)

```powershell
cd C:\Source\Repos\STOM\STOM.App
dotnet run
# → Listening on http://localhost:5173
```

### Step 4: Open Browser

- Navigate to: **<http://localhost:5173>**
- Choose role: Scorer, Admin, or SuperAdmin

---

## 🏗️ Architecture

```text
STOM.API (Port 5022)           STOM.App (Port 5173)
├── Controllers/               ├── Pages/
│   ├── GamesController        │   ├── Index.razor
│   └── AdminController        │   ├── Scorer/
├── Services/                  │   ├── Admin/
├── Models/                    │   └── SuperAdmin/
├── Data/                      ├── Services/
│   └── StomDbContext.cs       │   ├── ApiService.cs
└── Program.cs                 │   └── AuthService.cs
                               └── Program.cs

Database: STOM_DB (SQL Server LocalDB)
```

---

## 🔌 API Endpoints

| Method | Endpoint                 | Purpose                                 |
|--------|--------------------------|-----------------------------------------|
| GET    | `/api/games`             | List all games                          |
| GET    | `/api/games/{id}`        | Get game details                        |
| POST   | `/api/games/{id}/scores` | Add score to game                       |
| GET    | `/api/admin/audit-logs`  | View admin operations (SuperAdmin only) |

**Example:**

```powershell
# Get all games
curl http://localhost:5022/api/games

# Add score to game 1
curl -X POST http://localhost:5022/api/games/1/scores `
  -H "Content-Type: application/json" `
  -d '{"value": 100}'
```

---

## 🛠️ Common Commands

```powershell
# Build solution
cd C:\Source\Repos\STOM
dotnet build

# Restore packages
dotnet restore

# Run tests
dotnet test

# Create new EF migration
cd STOM.API
dotnet ef migrations add YourMigrationName

# Apply migrations manually
dotnet ef database update

# Check health
curl http://localhost:5022/api/health/status
```

---

## 🐛 Troubleshooting

| Issue                         | Solution                                                      |
|-------------------------------|---------------------------------------------------------------|
| **Port 5022 in use**          | Kill process: `Get-Process dotnet \| Stop-Process -Force`     |
| **Can't connect to database** | Start LocalDB: `sqllocaldb start mssqllocaldb`                |
| **Blazor app won't load**     | Clear browser cache (Ctrl+Shift+Delete) and refresh           |
| **Migrations fail**           | Delete database folder and restart: `dotnet ef database drop` |

---

## 📁 Project Files

| File                               | Purpose                         |
|------------------------------------|---------------------------------|
| `README.md`                        | Full architecture documentation |
| `QUICKSTART.md`                    | Quick start guide               |
| `SETUP_GUIDE.md`                   | This file - setup instructions  |
| `docs/skills/dev-request-skill.md` | Development request guidelines  |

---

## 📝 Development Workflow

1. **Create a feature branch**: `git checkout -b feature/my-feature`
2. **Make changes** to STOM.API and/or STOM.App
3. **Test locally**:
   - Run both projects (see Quick Start)
   - Use API endpoints to verify changes
   - Check browser console for errors
4. **Commit and push**: `git push origin feature/my-feature`
5. **Create pull request** on GitHub

---

## 🔐 Security Notes

- Role-based access control: Scorer, Admin, SuperAdmin
- SuperAdmin endpoints require authentication
- Audit logs track all admin operations
- Use connection string from appsettings.json (never hardcode)

---

## 📚 Documentation

- **Full Architecture**: See `README.md`
- **Database Setup**: Database initializes automatically on startup
- **Offline Support**: Blazor WASM includes IndexedDB support
- **Deployment**: Use Azure App Service or similar PaaS

---

**Last Updated:** November 28, 2026  
**Status:** ✅ Ready for Development
