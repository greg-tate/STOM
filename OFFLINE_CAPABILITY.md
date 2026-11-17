# STOM Offline Capability - Test & Usage Guide

## 🎯 Overview

STOM now supports **complete offline functionality** for game scoring pages. Scorers can:
- ✅ View games while offline (cached from last session)
- ✅ Submit scores while offline (queued in local storage)
- ✅ See which scores are pending sync
- ✅ Auto-sync when connection is restored
- ✅ Works great on mobile devices

## 🏗️ Architecture

### Services Implemented

1. **OfflineCapabilityService**
   - Detects online/offline status changes
   - Subscribes to `window.online` and `window.offline` events
   - Provides `IsOnline` and `IsOffline` properties

2. **CacheService**
   - Caches game data in `localStorage`
   - 1-hour cache expiration
   - Automatic fallback when offline
   - Methods:
     - `GetCachedGamesAsync()` / `CacheGamesAsync()`
     - `GetCachedGameAsync()` / `CacheGameAsync()`

3. **OfflineStorageService**
   - Persists pending scores in `localStorage` as JSON
   - Manages `PendingScore` objects with sync status
   - Survives browser refresh/app restart
   - Methods:
     - `AddPendingScoreAsync()` - Queue a score
     - `MarkSyncedAsync()` - Mark as synced
     - `RemoveSyncedAsync()` - Clean up synced scores
     - `GetPendingScores()` - Get unsync'd scores

4. **Enhanced ApiService**
   - Detects network errors and queues instead of failing
   - Falls back to cached data for GET requests
   - Implements `SyncPendingScoresAsync()` to upload queued scores
   - Emits `SyncStatusChanged` events

5. **Enhanced SyncService**
   - Coordinates sync operations
   - Listens for connection restored events
   - Auto-syncs pending scores when back online
   - Tracks pending count

## 🧪 Testing Offline Capability

### Method 1: Chrome DevTools (Recommended)
1. Open http://localhost:5049 in Chrome
2. Navigate to a game (/score/1)
3. Open DevTools → Network tab
4. Check "Offline" checkbox ⚠️
5. Try submitting a score
6. Notice:
   - Alert appears: "You are offline. Scores will be queued..."
   - Button says "Submit (Offline)"
   - Score appears with "⏳ Pending" badge
   - Layout shows "🌐 Offline" and "📤 N pending"
7. Uncheck "Offline" in DevTools
8. Notice:
   - Layout shows "🌐 Online"
   - Scores automatically sync
   - "✓ Synced" badges appear on pending scores

### Method 2: Network Throttling (Advanced)
1. DevTools → Network tab
2. Throttle connection (Slow 3G or Offline)
3. Try page navigation and scoring
4. Observe caching behavior

### Method 3: Manual Browser Offline Mode
1. F12 → Network tab
2. ☑️ "Offline" checkbox
3. Try submitting scores
4. See pending queue in browser console:
   ```javascript
   JSON.parse(localStorage.getItem('stom_pending_scores'))
   ```

## 📊 UI Indicators

### Top Navigation Bar
- **Online:** 🌐 Green "Online" badge
- **Offline:** 🌐 Yellow "Offline" badge
- **Pending Scores:** 📤 Blue badge showing count
- **Sync Status:** Small text showing sync state

### Scorer Page
- **Offline Alert:** Yellow warning box
- **Submit Button:** Changes text to "Submit (Offline)"
- **Score Table:**
  - Green "✓ Synced" badge for uploaded scores
  - Yellow "⏳ Pending" badge for queued scores
  - Row highlighted in yellow for pending items

### Sync Now Button
- Appears only when pending scores exist
- Manual sync trigger
- Shows sync progress

## 🔄 Sync Flow

```
User submits score
  ↓
Check if online
  ├─ YES: POST to API → Server saves → Remove from queue
  └─ NO: Add to localStorage queue → Show pending badge
  
Connection restored (auto-detect)
  ↓
Trigger sync
  ↓
For each pending score:
  POST to API → Mark synced
  ↓
Success: Remove from storage
Remaining: Show remaining count
```

## 💾 localStorage Schema

### Pending Scores (stom_pending_scores)
```json
[
  {
    "id": "uuid-string",
    "gameId": 1,
    "value": 50,
    "createdAt": "2024-11-17T22:30:00Z",
    "synced": false
  }
]
```

### Game Cache (stom_game_*)
```json
{
  "id": 1,
  "name": "Game Name",
  "createdAt": "2024-11-17T20:00:00Z",
  "scores": [...]
}
```

### Cache Timestamps (stom_cache_time_*)
- ISO 8601 string of when data was cached
- Used for 1-hour expiration logic

## 🚀 Usage Examples

### As a Scorer (UI Usage)
1. Go to http://localhost:5049
2. Select a game to score
3. If offline:
   - Enter score and submit
   - Score appears with pending badge
   - Come back online
   - Scores automatically sync
4. If online:
   - Standard scoring flow
   - Instant server confirmation

### Developer Console (Testing)
```javascript
// Check pending scores
JSON.parse(localStorage.getItem('stom_pending_scores'))

// Check game cache
JSON.parse(localStorage.getItem('stom_game_1'))

// Check online status
navigator.onLine

// Clear all offline data
localStorage.clear()

// Get all storage keys
Object.keys(localStorage)
```

## 🔧 Configuration

### Cache Duration
Edit `CacheService.cs`:
```csharp
private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
```

### Storage Key Prefixes
- Pending scores: `stom_pending_scores`
- Game cache: `stom_game_{gameId}`
- All cache: `stom_games_cache`
- Timestamps: `stom_cache_time_{key}`

### Service Registration
All services are auto-registered in `Program.cs`:
```csharp
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<OfflineStorageService>();
builder.Services.AddScoped<OfflineCapabilityService>();
```

## 📱 Mobile Testing

### iPhone Safari
1. Open http://localhost:5049 (from computer IP)
2. Airplane mode: ON
3. Submit scores (should queue)
4. Airplane mode: OFF
5. Scores sync automatically ✅

### Android Chrome
1. Same flow as iPhone
2. Can also test with DevTools remote debugging

## 🛠️ Service Worker

A service worker is registered (`wwwroot/service-worker.js`):
- Caches static assets
- Network-first strategy for GET requests
- Falls back to `index.html` for failed navigations
- Useful for PWA installability

## 📋 Implementation Files

**New Services:**
- `Services/OfflineCapabilityService.cs` - 44 lines
- `Services/CacheService.cs` - 85 lines
- `Services/OfflineStorageService.cs` - 103 lines

**Enhanced Services:**
- `Services/ApiService.cs` - Added caching + offline queue
- `Services/SyncService.cs` - Rewired with new services

**UI Updates:**
- `Pages/Scorer/GameScore.razor` - Added offline indicators
- `Layout/MainLayout.razor` - Added sync status bar

**Client Scripts:**
- `wwwroot/offline-support.js` - Browser API integration
- `wwwroot/service-worker.js` - PWA support

**Other:**
- `wwwroot/index.html` - Script registration

## ✅ Tested Features

- ✅ Submit scores while offline
- ✅ Scores persist in localStorage
- ✅ Scores sync when online
- ✅ Game data cached for offline access
- ✅ UI shows offline/online status
- ✅ Pending scores show 1-hour cache
- ✅ Browser refresh preserves queue
- ✅ Auto-sync on connection restored
- ✅ Manual "Sync Now" button works

## 🐛 Troubleshooting

### Scores not syncing
1. Check DevTools Console for errors
2. Verify API is running: http://localhost:5022/api/games
3. Check Network tab for failed POST requests
4. Clear localStorage and try again

### Cache not working
1. Verify cache service initialized
2. Check localStorage keys in DevTools
3. Confirm cache timestamps haven't expired

### Offline status not changing
1. Verify offline-support.js loaded
2. Check DevTools Console for JS errors
3. Try F12 refresh (Ctrl+Shift+R)

### Service Worker issues
1. Check DevTools → Application → Service Workers
2. Unregister and refresh if corrupted
3. Check browser console for registration errors

## 📚 Related Documentation

- [STOM README.md](../README.md) - Main project docs
- [QUICKSTART.md](../QUICKSTART.md) - Getting started
- [DB_SETUP.md](../DB_SETUP.md) - Database setup

## 🎓 Key Learnings

1. **LocalStorage vs SessionStorage**
   - Using localStorage because scores need to survive app restart
   - SessionStorage would be lost on close

2. **Network Detection**
   - Using `navigator.onLine` - works across all modern browsers
   - Combined with event listeners for real-time updates

3. **Cache Expiration**
   - Storing timestamp with each cached item
   - 1-hour default to balance freshness vs bandwidth

4. **Score Queueing**
   - Using unique IDs (GUIDs) for each pending score
   - Synced flag prevents duplicate uploads
   - Survives page refresh

5. **UI State Synchronization**
   - Events emitted from services to update UI
   - Blazor StateHasChanged() called on event handlers
   - Clean separation of concerns

---

**Version:** 1.0  
**Date:** November 17, 2024  
**Author:** Copilot  
