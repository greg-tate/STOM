# STOM Quick Reference

## Connection Strings

| Environment | Connection String                                         |
| ----------- | --------------------------------------------------------- |
| Development | `Server=.;Database=STOM_DB_Dev;Integrated Security=true;` |
| Production  | `Server=.;Database=STOM_DB;Integrated Security=true;`     |

---

## API Endpoints

| Method | Endpoint                 | Purpose                      |
| ------ | ------------------------ | ---------------------------- |
| GET    | `/api/games`             | List all games               |
| GET    | `/api/games/{id}`        | Get game details             |
| POST   | `/api/games/{id}/scores` | Add score to game            |
| GET    | `/api/admin/audit-logs`  | View audit logs (SuperAdmin) |

---

## Ports

| Service  | Port | URL                     |
|----------|------|-------------------------|
| STOM.API | 5022 | <http://localhost:5022> |
| STOM.App | 5173 | <http://localhost:5173> |

---

## Essential Commands

```powershell
# Start API
cd STOM.API && dotnet run

# Start App
cd STOM.App && dotnet run

# Build solution
dotnet build

# Create migration
cd STOM.API && dotnet ef migrations add YourMigrationName

# Apply migrations
dotnet ef database update

# View database
sqlcmd -S "(localdb)\mssqllocaldb" -d STOM_DB
> SELECT * FROM Games;
> GO
```

---

## Health Check

```powershell
# Check API status
curl http://localhost:5022/api/health/status
```

---

## See Also

See [SETUP_GUIDE.md](SETUP_GUIDE.md) for detailed setup instructions.
