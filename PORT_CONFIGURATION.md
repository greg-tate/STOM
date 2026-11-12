# STOM Port Configuration Guide

## Current Port Configuration

### STOM.API (Backend REST API)

**File:** `STOM.API/Properties/launchSettings.json`

| Protocol | Port | URL | Profile |
|----------|------|-----|---------|
| **HTTP** | 5022 | `http://localhost:5022` | `http` |
| **HTTPS** | 7022 | `https://localhost:7022` | `https` |

**API Endpoints:**
- Health Status: `http://localhost:5022/api/health/status`
- Database Info: `http://localhost:5022/api/health/database`
- OpenAPI Docs: `https://localhost:7022/openapi/v1.json` (Development only)

### STOM.App (Blazor WebAssembly Client)

**File:** `STOM.App/Properties/launchSettings.json`

| Protocol | Port | URL | Profile |
|----------|------|-----|---------|
| **HTTP** | 5049 | `http://localhost:5049` | `http` |
| **HTTPS** | 7102 | `https://localhost:7102` | `https` |

**Client:**
- Web App: `http://localhost:5049` (opens browser automatically)
- Web App: `https://localhost:7102` (encrypted)

---

## API Connection Configuration

### Current Setting in STOM.App

**File:** `STOM.App/Program.cs`

```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5022") 
});
```

**Issue:** The Blazor app is hardcoded to connect to `http://localhost:5022`

**Good News:** ✅ This matches STOM.API's HTTP port!

---

## Running Both Projects

### Full Stack Setup

**Terminal 1 - Start API (Port 5022):**
```bash
cd C:\Source\Repos\STOM\STOM.API
set ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Outputs:
# Building...
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5022
#       Now listening on: https://localhost:7022
```

**Terminal 2 - Start Blazor App (Port 5049):**
```bash
cd C:\Source\Repos\STOM\STOM.App
dotnet run

# Outputs:
# Building...
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5049
#       Now listening on: https://localhost:7102
# 
# Browser will open: http://localhost:5049
```

---

## Testing URLs

### API Testing

```bash
# Check if API is running
curl http://localhost:5022/api/health/status

# Expected response:
# {"status":"Healthy","message":"All systems operational","timestamp":"2024-11-13T00:02:00Z"}

# Check database status
curl http://localhost:5022/api/health/database

# View OpenAPI documentation (HTTPS only)
# https://localhost:7022/openapi/v1.json
```

### Blazor App Testing

```bash
# Browser
http://localhost:5049

# App will connect to API at http://localhost:5022
```

---

## Port Usage Summary

```
┌─────────────────────────────────────┐
│      STOM Application Stack         │
├─────────────────────────────────────┤
│                                     │
│  STOM.API (Backend)                │
│  ├─ HTTP:  localhost:5022          │
│  ├─ HTTPS: localhost:7022          │
│  └─ Database: SQL Server (via TCP) │
│                                     │
│  STOM.App (Frontend)               │
│  ├─ HTTP:  localhost:5049          │
│  ├─ HTTPS: localhost:7102          │
│  └─ Connects to: localhost:5022    │
│                                     │
└─────────────────────────────────────┘
```

---

## Port Availability Check

### Check if Ports are in Use

**PowerShell:**
```powershell
# Check port 5022 (API HTTP)
Test-NetConnection -ComputerName localhost -Port 5022

# Check port 7022 (API HTTPS)
Test-NetConnection -ComputerName localhost -Port 7022

# Check port 5049 (App HTTP)
Test-NetConnection -ComputerName localhost -Port 5049

# Check port 7102 (App HTTPS)
Test-NetConnection -ComputerName localhost -Port 7102
```

**Command Prompt:**
```cmd
netstat -ano | findstr :5022
netstat -ano | findstr :7022
netstat -ano | findstr :5049
netstat -ano | findstr :7102
```

---

## Configuration Details

### STOM.API launchSettings.json

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5022",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7022;http://localhost:5022",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### STOM.App launchSettings.json

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5049",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7102;http://localhost:5049",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## Troubleshooting

### Issue: "Address already in use" Error

**Cause:** Port is already being used

**Solution:**
```powershell
# Find what's using the port
Get-Process -Id (Get-NetTCPConnection -LocalPort 5022).OwningProcess

# Kill the process (if safe)
Stop-Process -Id <PID> -Force

# Or use different ports (see Customization below)
```

### Issue: Blazor App Can't Connect to API

**Cause:** API not running or wrong port configured

**Solution:**
1. Verify API is running: `curl http://localhost:5022/api/health/status`
2. Check Blazor app's API address in `STOM.App/Program.cs`
3. Ensure CORS is enabled in API (already configured)

### Issue: HTTPS Certificate Warning

**Cause:** Self-signed development certificate

**Solution:** 
- For development, ignore the warning
- For production, use proper SSL certificates

---

## Customizing Ports (Optional)

### Change API Port

Edit `STOM.API/Properties/launchSettings.json`:
```json
"applicationUrl": "http://localhost:5555"
```

### Change App Port

Edit `STOM.App/Properties/launchSettings.json`:
```json
"applicationUrl": "http://localhost:3000"
```

### Update App's API Connection

Edit `STOM.App/Program.cs`:
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5555")  // Updated port
});
```

---

## Quick Start Commands

### One-Line Setup

**Terminal 1:**
```bash
cd C:\Source\Repos\STOM\STOM.API && set ASPNETCORE_ENVIRONMENT=Development && dotnet run
```

**Terminal 2:**
```bash
cd C:\Source\Repos\STOM\STOM.App && dotnet run
```

### Verify Everything Works

```bash
# Test API
curl http://localhost:5022/api/health/status

# Open Blazor App (should auto-open)
start http://localhost:5049
```

---

## Production Considerations

### For Production Deployment

**Recommended Ports:**
- API: Port 80 (HTTP) / 443 (HTTPS)
- App: Typically served from API or separate web server
- DNS: Use domain names instead of localhost

**Production Connection String:**
```json
"BaseAddress": "https://api.yourdomain.com"
```

**HTTPS Configuration:**
```json
"applicationUrl": "https://0.0.0.0:443"
```

Requires proper SSL certificates from trusted CA.

---

## Summary

✅ **STOM.API** runs on port **5022** (HTTP) / **7022** (HTTPS)
✅ **STOM.App** connects to **localhost:5022** (correctly configured)
✅ **STOM.App** runs on port **5049** (HTTP) / **7102** (HTTPS)
✅ **CORS enabled** to allow cross-origin API calls
✅ **No port conflicts** - each service has unique ports

**You're ready to run the full stack!** 🚀

