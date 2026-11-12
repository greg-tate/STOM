# STOM Database Setup & Sync Strategy

## Overview

STOM uses a **hybrid dual-database architecture** for robust offline-capable tournament scoring:

- **SQL Server (STOM_DB)** - Central backend, source of truth
- **IndexedDB** - Browser-based offline cache for Blazor client

This approach enables seamless offline functionality while maintaining data consistency and centralized management.

---

## Architecture

```
┌─────────────────────────────┐
│     SQL Server (STOM_DB)    │  ← Master Data
│   - Tournaments             │
│   - Players                 │
│   - Games & Scores          │
│   - Audit Logs              │
│   - Sync Metadata           │
└──────────────┬──────────────┘
               │
        API (REST/SignalR)
               │
┌──────────────▼──────────────┐
│   Blazor WebApp Client      │
│  (STOM.Client)              │
├─────────────────────────────┤
│       IndexedDB Cache       │
│  - Tournaments (read-only)  │
│  - Players (read-only)      │
│  - Scores (local edits)     │
│  - Sync Queue (pending ops) │
│  - Timestamps & ETags       │
├─────────────────────────────┤
│    Service Worker           │
│  - Offline detection        │
│  - Background sync          │
│  - Cache management         │
└─────────────────────────────┘
```

---

## Database Setup

### SQL Server Configuration

**Database Name:** `STOM_DB`

**Connection String:**
```
Server=(localdb)\mssqllocaldb;Database=STOM_DB;Integrated Security=true;
```

**Location:** `appsettings.json`

**Automatic Creation:**
- Database is created automatically on first application startup
- Migrations run via `dbContext.Database.Migrate()` in `Program.cs`
- No manual SQL Server setup required (LocalDB handles it)

**Requirements:**
- SQL Server 2025 or LocalDB installed
- Named pipes enabled for LocalDB connectivity
- Visual Studio or SQL Server Developer Edition recommended

### Migrations

**Current Migrations:**
- Location: `STOM.API/Migrations/`
- Initial: `20261109133537_InitialCreate.cs`

**Adding New Migrations:**
```bash
cd STOM.API
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

---

## IndexedDB Structure

### Stores (Collections)

#### 1. **tournaments**
```javascript
{
  id: string,              // Primary key
  name: string,
  date: string,            // ISO date
  status: string,          // "draft" | "active" | "completed"
  format: string,          // Tournament format
  _syncState: string,      // "synced" | "dirty" | "deleted"
  _lastModified: number,   // Timestamp (ms)
  _etag: string            // Server version hash
}

Index: id (primary)
Index: _lastModified (for sync queries)
```

#### 2. **players**
```javascript
{
  id: string,
  tournamentId: string,
  name: string,
  email: string,
  rating: number,
  _syncState: string,
  _lastModified: number,
  _etag: string
}

Indexes: id (primary), tournamentId, _lastModified
```

#### 3. **games**
```javascript
{
  id: string,
  tournamentId: string,
  player1Id: string,
  player2Id: string,
  round: number,
  status: string,          // "pending" | "in_progress" | "completed"
  _syncState: string,
  _lastModified: number,
  _etag: string
}

Indexes: id (primary), tournamentId, _lastModified
```

#### 4. **scores**
```javascript
{
  id: string,
  gameId: string,
  playerId: string,
  points: number,
  penalty: number,
  notes: string,
  _syncState: string,
  _lastModified: number,
  _etag: string,           // May be null for local-only edits
  _originalServerId: string // Server ID if exists
}

Indexes: id (primary), gameId, _lastModified
```

#### 5. **syncQueue**
```javascript
{
  id: string,              // UUID
  operation: string,       // "CREATE" | "UPDATE" | "DELETE"
  entityType: string,      // "score" | "game" | "tournament"
  entityId: string,
  payload: object,         // Full entity data
  timestamp: number,       // When queued (ms)
  attempt: number,         // Retry count
  status: string           // "pending" | "syncing" | "failed"
}

Index: timestamp (for FIFO processing)
Index: status (for retry queries)
```

#### 6. **syncMetadata**
```javascript
{
  key: string,             // "lastSync" | "lastSyncTournament_{id}"
  value: string,           // ISO timestamp
  _updated: number
}

Index: key (primary)
```

---

## Sync Strategy

### Sync Flow

#### **Initial Load (Online)**
1. Blazor app starts, detects online status
2. Fetch tournament & player data from API
3. Store in IndexedDB with `_syncState: "synced"`
4. Cache latest `_etag` and timestamp
5. User can now work offline

#### **During Offline Use**
1. User creates/edits scores locally
2. Changes stored in IndexedDB with `_syncState: "dirty"`
3. Operation queued in `syncQueue` table
4. UI shows "offline mode" indicator
5. App continues responding instantly (no server calls)

#### **Reconnection (Online Detection)**
1. Service Worker detects online status via `online` event
2. Trigger background sync: process `syncQueue` FIFO
3. For each queued operation:
   - POST/PUT/DELETE to API endpoint
   - Include local `_etag` in request header `If-Match`
   - Handle response (success/conflict)
4. Update IndexedDB based on sync results
5. Remove synced items from queue

### Conflict Resolution

#### **Conflict Detection**
- Server returns `409 Conflict` if `If-Match` ETag doesn't match
- Indicates entity was modified by another user while offline

#### **Resolution Strategy: Last-Write-Wins**

**For Scores (Recommended):**
```
If local _lastModified > server _lastModified:
  → Keep local version, force update to server
Else:
  → Fetch server version, show merge dialog to user
    - "Your change" vs "Server version"
    - Let user choose or merge
    - Update local & queue new sync
```

**For Tournament/Game Metadata:**
- Server version always wins (read-only for offline users)
- User notified: "Tournament updated by administrator, refreshing..."

### Sync Endpoints (New API Routes)

These endpoints need to be added to `STOM.API`:

#### **POST /api/sync/scores**
```csharp
[HttpPost("api/sync/scores")]
public async Task<IActionResult> SyncScores([FromBody] List<ScoreSyncRequest> changes)
{
    // Process each score change
    // Return 409 Conflict if ETag mismatch
    // Update server DB
    // Return new ETags and timestamps
}
```

**Request:**
```json
[
  {
    "id": "score-uuid",
    "gameId": "game-uuid",
    "points": 25,
    "etag": "server-etag-hash",
    "lastModified": 1699608000000
  }
]
```

**Response (200):**
```json
{
  "synced": [
    { "id": "score-uuid", "etag": "new-etag", "serverTimestamp": 1699608003000 }
  ],
  "conflicts": [
    {
      "id": "score-uuid",
      "reason": "Entity modified by another user",
      "serverVersion": { /* full entity */ },
      "clientVersion": { /* full entity */ }
    }
  ]
}
```

#### **GET /api/tournaments/{id}/full**
Fetch complete tournament data with all players, games, scores for offline cache

#### **GET /api/sync/changes?since={timestamp}**
Fetch only changed entities since last sync (for incremental sync)

---

## Implementation Overview

### Phase 1: Setup IndexedDB Layer

**Files to create:**
- `STOM.Client/Services/IndexedDbService.cs` - Wrapper around Dexie.js or similar
- `STOM.Client/Models/SyncQueue.cs` - Sync operation model
- `STOM.Client/wwwroot/js/indexed-db.js` - Browser IndexedDB operations

**Key Classes:**
```csharp
public class IndexedDbService
{
    // Initialize stores
    Task InitializeAsync();
    
    // CRUD operations
    Task<T> GetAsync<T>(string storeName, string key);
    Task<IEnumerable<T>> GetAllAsync<T>(string storeName);
    Task<string> SetAsync<T>(string storeName, T item);
    Task DeleteAsync(string storeName, string key);
    
    // Sync operations
    Task QueueSyncAsync(SyncOperation operation);
    Task<IEnumerable<SyncOperation>> GetPendingSyncAsync();
    Task MarkSyncedAsync(string operationId);
}
```

### Phase 2: Add Service Worker

**File:** `STOM.Client/wwwroot/service-worker.js`

**Responsibilities:**
- Intercept fetch requests
- Serve from cache if offline
- Detect online/offline transitions
- Trigger sync when reconnected

```javascript
// Pseudo-code
self.addEventListener('fetch', (event) => {
  if (isOnline) {
    // Pass through to server
  } else if (isCacheableRequest(event)) {
    // Serve from cache
  }
});

self.addEventListener('online', () => {
  // Trigger sync from IndexedDB
});
```

### Phase 3: UI/UX Integration

**Blazor Components to Update:**
- Add offline mode indicator to layout
- Queue status badge (pending operations count)
- Sync progress dialog
- Conflict resolution UI

**Example UI:**
```
[OFFLINE MODE] - 3 changes pending
┌────────────────────────────┐
│ Syncing... 2/3             │
│ ████████░░                 │
└────────────────────────────┘
```

### Phase 4: API Endpoints

**Add to `STOM.API/Controllers`:**

1. `SyncController.cs` - Handle batch sync operations
2. Update `ScoresController.cs` - Add ETag support
3. Add `FullExportController.cs` - Export tournament for offline use

**Middleware to add:**
- ETag generation/validation
- Sync timestamp tracking
- Conflict detection

---

## Testing Sync Strategy

### Unit Tests
- `Tests/Services/IndexedDbServiceTests.cs`
- `Tests/Services/SyncServiceTests.cs`
- Conflict resolution logic
- Queue ordering (FIFO)

### Integration Tests
1. **Offline Scenario:**
   - Disable network (DevTools)
   - Make local changes
   - Verify IndexedDB state
   - Re-enable network
   - Verify sync completes

2. **Conflict Scenario:**
   - User A: offline, changes score locally
   - User B: online, updates same score on server
   - User A reconnects
   - Verify conflict detected, UI shows merge dialog

3. **Data Consistency:**
   - Multiple offline users
   - Sync in different orders
   - Verify server state is consistent

---

## Security Considerations

### Data Protection
- IndexedDB is **not encrypted** by default
  - On shared devices, user can see other users' data
  - Consider Service Worker cache isolation per user
  
- Use **httpOnly cookies** or **secure tokens** for API auth
  - Don't store auth tokens in IndexedDB

### Offline Limitations
- Users can only see data they've cached
- Cannot access new tournaments created while offline
- Audit logs may be incomplete

---

## Migration Path

### Current State
- ✅ SQL Server backend ready
- ✅ Blazor client created
- ❌ No offline support

### Recommended Rollout
1. **Week 1-2:** Implement IndexedDB layer + basic caching
2. **Week 2-3:** Add Service Worker for offline assets
3. **Week 3-4:** Implement sync endpoints + queue processing
4. **Week 4+:** Testing, conflict resolution, performance tuning

### Compatibility
- Works with existing SQL Server setup (no migration)
- No breaking changes to current API
- Adds new `/api/sync/*` endpoints (backward compatible)

---

## References

### Libraries & Tools
- **IndexedDB API:** [MDN Docs](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API)
- **Dexie.js:** Simpler IndexedDB wrapper (optional)
- **Service Worker API:** [MDN Docs](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- **Workbox:** Google's Service Worker library (optional)

### Related Features
- Entity Framework Core ETag support
- SignalR for real-time multi-user updates (future)
- Background Sync API for offline persistence (future)

---

## Questions & Decisions

| Question | Decision | Rationale |
|----------|----------|-----------|
| Conflict resolution strategy? | Last-write-wins for scores, server-wins for metadata | Simple, matches tournament scoring workflow |
| Max offline period? | Unlimited (until reconnect) | Assumption: venue WiFi will be available eventually |
| Encryption? | Not initially (security review needed) | Complexity vs. threat model assessment |
| Multi-device sync? | Not supported initially | Scope out for Phase 2 |
| Real-time collaboration? | Queued updates (not live) | Sufficient for tournament use case |

---

## Next Steps

1. ✏️ Review this architecture with team
2. 🗂️ Create IndexedDB service layer
3. 🔄 Implement Service Worker
4. 🚀 Add sync endpoints to API
5. 🧪 Integration testing
6. 📝 Update deployment docs

