# STOM Database - Quick Reference

## Start the Application

```bash
cd C:\Source\Repos\STOM\STOM.API
dotnet run
```

✅ Database is created automatically on startup

---

## Connection Details

| Setting | Value |
|---------|-------|
| **Server** | `(localdb)\mssqllocaldb` |
| **Database** | `STOM_DB` (or `STOM_DB_Dev`) |
| **Auth** | Windows Authentication |

---

## Health Check

```bash
# Check status
curl http://localhost:5000/api/health/status

# Database info
curl http://localhost:5000/api/health/database
```

---

## View Migrations

```bash
cd STOM.API
dotnet ef migrations list
```

---

## Create New Migration

```bash
cd STOM.API
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

---

## Add to Database Model

1. Create new class in `STOM.API/Models/`
2. Add `DbSet<YourModel> YourModels { get; set; }` to `StomDbContext`
3. Create migration: `dotnet ef migrations add AddYourModel`
4. Update database: `dotnet ef database update`

---

## Connect in SQL Server Management Studio

- **Server:** `(localdb)\mssqllocaldb`
- **Database:** `STOM_DB` or `STOM_DB_Dev`
- **Authentication:** Windows

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Can't connect to database | Run `sqllocaldb start` |
| Migrations not applied | Run `dotnet ef database update` |
| Need to reset | Delete database, restart app |

---

## Key Files

- **Configuration:** `STOM.API/appsettings.json`
- **Code:** `STOM.API/Program.cs`
- **DB Context:** `STOM.API/Data/StomDbContext.cs`
- **Initialization:** `STOM.API/Services/DatabaseInitializationService.cs`

---

## Full Documentation

- **Setup Guide:** `DATABASE_SETUP_GUIDE.md`
- **Architecture:** `DB_SETUP.md`
- **Changes Made:** `CHANGES.md`
- **SQL Reference:** `SQL_SERVER_SETUP.sql`

