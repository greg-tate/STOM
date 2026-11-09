# STOM Solution - Hybrid 3-Project Architecture

This is the refactored ASTM→STOM solution implementing a **Blazor WebAssembly hybrid 3-project architecture** for tournament scoring and management.

## Solution Structure

```
STOM/
├── STOM.API/                 ← ASP.NET Core REST + Admin API (Port 5022)
│   ├── Controllers/
│   │   ├── GamesController.cs       (Scorer endpoints)
│   │   └── AdminController.cs       (SuperAdmin endpoints)
│   ├── Models/
│   ├── Data/
│   │   └── StomDbContext.cs         (EF Core DbContext)
│   └── Services/
│       ├── AuditService.cs          (Log admin operations)
│       ├── BracketGenerator.cs
│       └── PlayerImportService.cs
│
├── STOM.App/                 ← Blazor WASM PWA (Port 5173)
│   ├── Pages/
│   │   ├── Index.razor              (Home & Role Selection)
│   │   ├── Scorer/
│   │   │   └── GameScore.razor      (Score entry UI)
│   │   ├── Admin/
│   │   │   └── Dashboard.razor      (Admin dashboard)
│   │   └── SuperAdmin/
│   │       └── AdminConsole.razor   (Admin console + audit logs)
│   ├── Services/
│   │   ├── ApiService.cs
│   │   ├── AdminApiService.cs
│   │   ├── AuthService.cs
│   │   └── SyncService.cs
│   └── Models/
│
├── STOM.Database/            ← SQL Server scripts
│   ├── Tables/
│   ├── Views/
│   ├── StoredProcedures/
│   └── Scripts/
│
└── STOM.slnx                 ← Modern solution file
```

## Architectural Highlights

### Why Hybrid 3-Project?
- **3 projects** instead of 4 (simpler management)
- **Single PWA** for all roles (Scorer, Admin, SuperAdmin)
- **Code reuse** across all layers
- **Offline support** for mobile scorers (IndexedDB + Service Workers)
- **Audit trail** for all admin operations
- **Enterprise pattern** (REST API + role-based auth)

### Technology Stack
- **Backend**: ASP.NET Core 11 (REST API)
- **Frontend**: Blazor WebAssembly (PWA for all roles)
- **Database**: SQL Server 2025 LocalDB
- **Auth**: Role-based (Scorer, Admin, SuperAdmin)

## Development Workflow

### Step 1: Restore & Build
```powershell
cd C:\Source\Repos\ASTM
dotnet restore
dotnet build
```

### Step 2: Setup Database
```powershell
# Create LocalDB instance
sqlcmd -S "(localdb)\mssqllocaldb" -i STOM.Database\Scripts\001-CreateDatabase.sql
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB -i STOM.Database\Scripts\002-CreateTables.sql
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB -i STOM.Database\Scripts\003-SeedData.sql
```

### Step 3: Run API & App

**Terminal 1 - API (Port 5022)**:
```powershell
cd C:\Source\Repos\ASTM\STOM.API
dotnet run
# → http://localhost:5022
```

**Terminal 2 - Blazor App (Port 5173)**:
```powershell
cd C:\Source\Repos\ASTM\STOM.App
dotnet run
# → http://localhost:5173
```

### Step 4: Access the Application
1. Open browser → http://localhost:5173
2. Choose role (Scorer, Admin, or SuperAdmin)
3. Navigate to corresponding UI:
   - **Scorer**: Enter scores (offline-capable)
   - **Admin**: View dashboards and standings
   - **SuperAdmin**: Access admin console and audit logs

## API Endpoints

### Scorer Endpoints (Public - Always Available)
```
GET    /api/games              - List all games
GET    /api/games/{id}         - Get game details
POST   /api/games/{id}/scores  - Add score to game
```

### Admin Endpoints (SuperAdmin Only - Audited)
```
POST   /api/admin/competitions/create              - Create competition
POST   /api/admin/competitions/{id}/brackets       - Generate brackets
POST   /api/admin/players/import-bulk              - Import players
POST   /api/admin/scores/{id}/correction           - Correct score
GET    /api/admin/audit-logs                       - Get audit logs
GET    /api/admin/audit-logs?since={date}          - Get logs since date
```

## Key Features

✅ **Role-Based UI**: Single app, different interfaces based on login role  
✅ **Offline Scoring**: IndexedDB + Service Workers for mobile scorers  
✅ **Admin Auditing**: Every admin operation logged with timestamp, user, IP  
✅ **PWA Installable**: Install to home screen from Chrome/Firefox  
✅ **Shared Models**: Common C# classes across API and app (no DTOs needed)  
✅ **Enterprise Security**: OAuth-ready, role-based authorization  

## Configuration

### Connection String (STOM.API/appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=STOM_DB;Integrated Security=true;"
  }
}
```

### API Base URL (STOM.App/Program.cs)
```csharp
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5022") });
```

## Development Phases

| Phase | Focus | Status |
|-------|-------|--------|
| Phase 0 | Solution structure & projects | ✅ Complete |
| Phase 1 | API development & models | ✅ Complete |
| Phase 2 | Scorer UI & offline support | ⏳ Next |
| Phase 3 | Admin UI & dashboards | ⏳ Next |
| Phase 4 | SuperAdmin console & audit logs | ⏳ Next |
| Phase 5 | Testing & deployment | ⏳ Next |

## Security Model

### Authentication
- Login returns JWT token with role (Scorer, Admin, SuperAdmin)
- Roles determine accessible UI pages and API endpoints
- SuperAdmin role required for /api/admin/* endpoints

### Audit Logging
All admin operations logged to AuditLogs table with:
- Timestamp (UTC)
- Action (Create, Update, Delete, etc.)
- PerformedBy (User email)
- EntityId (affected record ID)
- Details (operation details)
- IpAddress (request source)

### Authorization
- [Authorize] on API controllers enforces authentication
- [Authorize(Roles = "SuperAdmin")] on admin endpoints
- Client-side @attribute [Authorize(Roles = "SuperAdmin")] on pages

## Database Design

### Core Tables
- **Games**: Tournament games
- **Scores**: Individual scores entered by scorers
- **Tournaments**: Competitions
- **Players**: Users with assigned roles
- **AuditLogs**: Immutable record of admin operations

### Indexes
- Timestamp (DESC) on AuditLogs for fast query
- PerformedBy on AuditLogs for user activity tracking
- GameId (FK) on Scores for game lookup

## Testing Checklist

- [ ] Scorer: Enter scores offline, sync when online
- [ ] Admin: View dashboards and game standings
- [ ] SuperAdmin: Create competitions and view audit logs
- [ ] PWA: Install to home screen
- [ ] Offline: Verify IndexedDB caching
- [ ] Security: Verify admin endpoints require SuperAdmin role
- [ ] Audit: Verify all admin operations logged

## Troubleshooting

### API won't start
```powershell
# Kill any process on port 5022
Get-Process | Where-Object {$_.Name -match "dotnet"} | Stop-Process -Force
```

### Database connection failed
```powershell
# Verify LocalDB is running
sqllocaldb info
sqllocaldb start mssqllocaldb

# Recreate database
sqlcmd -S "(localdb)\mssqllocaldb" -i STOM.Database\Scripts\001-CreateDatabase.sql
```

### Blazor app won't load
```powershell
# Clear browser cache
# Ctrl+Shift+Delete → Clear all cache
# Then refresh http://localhost:5173
```

## Next Steps

1. Run `dotnet restore` to restore all NuGet packages
2. Execute database setup scripts in STOM.Database/Scripts/
3. Start STOM.API in Terminal 1
4. Start STOM.App in Terminal 2
5. Open http://localhost:5173 and begin testing

## References

- [Blazor WebAssembly Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/webassembly)
- [ASP.NET Core REST API Design](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

---

**Architecture**: Hybrid 3-Project (STOM.API + STOM.App + STOM.Database)  
**Status**: Phase 0 Complete, Phase 1-5 Ready for Implementation  
**Last Updated**: November 9, 2026
