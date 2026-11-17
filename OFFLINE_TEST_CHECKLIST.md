# STOM Offline Capability - Manual Test Checklist

## Pre-Test Setup
- [ ] API running on http://localhost:5022
- [ ] App running on http://localhost:5049
- [ ] Chrome/Edge with DevTools available
- [ ] At least one game exists in database

## Test 1: Online Scoring (Baseline)
1. [ ] Open http://localhost:5049
2. [ ] Navigate to home page
3. [ ] Observe: "🌐 Online" badge in top-right
4. [ ] Click on a game link (e.g., Game 1)
5. [ ] Expected: Game loads with scores table
6. [ ] Enter a score value (e.g., 100)
7. [ ] Click "Submit Score" button
8. [ ] Expected: 
   - Score appears in table with "✓ Synced" badge
   - No "Pending" counter shows
   - Button still says "Submit Score" (not "Submit (Offline)")

## Test 2: Offline Detection
1. [ ] With app still open, open Chrome DevTools (F12)
2. [ ] Go to Network tab
3. [ ] Check the "Offline" checkbox ⚠️
4. [ ] Observe main page:
   - [ ] "🌐 Offline" badge shows (yellow)
   - [ ] Alert message appears on scorer page: "You are offline..."
   - [ ] Submit button text changes to "Submit (Offline)"

## Test 3: Offline Scoring
1. [ ] While offline, enter a new score (e.g., 75)
2. [ ] Click "Submit (Offline)" button
3. [ ] Expected:
   - [ ] Score appears in table with "⏳ Pending" badge
   - [ ] Row is highlighted in yellow
   - [ ] "📤 1 pending" counter shows in top-right
   - [ ] "Sync Now" button appears
   - [ ] Submit button still works for more scores
4. [ ] Submit a few more scores (e.g., 50, 25)
5. [ ] Expected:
   - [ ] All appear with "⏳ Pending" badge
   - [ ] Counter shows "📤 3 pending"
   - [ ] Each has own row in table

## Test 4: Browser Storage Verification
1. [ ] Press F12 → Application tab
2. [ ] Left sidebar → Local Storage → http://localhost:5049
3. [ ] Find key: "stom_pending_scores"
4. [ ] [ ] Click on it and verify JSON shows:
   - 3 score objects
   - Each has id, gameId, value, createdAt, synced: false
5. [ ] Find cached game key: "stom_game_1" (adjust for game ID)
6. [ ] [ ] Verify it contains full game data with scores

## Test 5: Page Refresh Persistence
1. [ ] Reload page (Ctrl+R or F5)
2. [ ] Expected:
   - [ ] Still offline (badge shows "🌐 Offline")
   - [ ] All 3 pending scores still visible in table
   - [ ] "⏳ Pending" badges still present
   - [ ] "📤 3 pending" counter still shows
   - Game data still cached and visible

## Test 6: Reconnection and Auto-Sync
1. [ ] Uncheck "Offline" checkbox in DevTools (back online)
2. [ ] Observe:
   - [ ] "🌐 Online" badge appears (green)
   - [ ] "📤 N pending" counter starts syncing
   - [ ] After ~5 seconds, all pending scores should:
     - Badges change from "⏳ Pending" to "✓ Synced"
     - Rows no longer highlighted
   - [ ] Counter disappears (no pending scores)
   - [ ] "Sync Now" button disappears

## Test 7: Sync Status Messages
1. [ ] Go offline again
2. [ ] Submit 2 scores
3. [ ] Go back online
4. [ ] Observe main layout top-right:
   - [ ] Shows "Syncing X pending scores..." message
   - [ ] Updates to "All scores synced!" or "Synced X, Y pending"

## Test 8: Manual Sync Button
1. [ ] Go offline
2. [ ] Submit scores
3. [ ] Go online
4. [ ] Don't wait for auto-sync
5. [ ] [ ] Click "Sync Now" button in top-right
6. [ ] Expected:
   - Sync happens immediately
   - Pending badges become synced
   - Counter clears

## Test 9: Multiple Games Offline
1. [ ] Go offline
2. [ ] Navigate to Game 1 (/score/1)
3. [ ] Submit a score
4. [ ] Navigate to Game 2 (/score/2)
5. [ ] Submit a score
6. [ ] [ ] Verify pending counter shows "📤 2 pending"
7. [ ] Go back online
8. [ ] [ ] Verify both games sync correctly
9. [ ] Check both game pages - both should show synced badges

## Test 10: Cache Duration
1. [ ] Clear localStorage manually:
   - F12 → Application → Local Storage
   - Delete keys starting with "stom_"
2. [ ] Go online
3. [ ] Load a game page
4. [ ] [ ] Check localStorage - new cache should exist
5. [ ] Go offline
6. [ ] [ ] Game still visible (cached)
7. [ ] Wait 1 hour (or manually edit timestamp in storage)
8. [ ] Go offline fresh browser session
9. [ ] [ ] Old cache shouldn't load (expired)

## Test 11: Error Handling
1. [ ] Stop the API server (or unplug network)
2. [ ] Be online in app
3. [ ] Try to load a game
4. [ ] Expected: 
   - Fallback to cached data if available
   - Or show "Game not found" if no cache
   - Doesn't crash

## Test 12: Service Worker Registration (Bonus)
1. [ ] F12 → Application → Service Workers
2. [ ] [ ] Should see "service-worker" registered
3. [ ] [ ] Status shows "activated and running"
4. [ ] Go offline
5. [ ] [ ] Try refreshing page
6. [ ] [ ] Should load from service worker cache

## Test 13: Mobile Simulation
1. [ ] F12 → More Tools → Network Conditions
2. [ ] Or: Device Toolbar (Ctrl+Shift+M)
3. [ ] Set to mobile device (iPhone 12, etc.)
4. [ ] [ ] Offline indicator and UI should work on small screen
5. [ ] [ ] Submit button accessible
6. [ ] [ ] Pending counter visible

## Test 14: Console Errors
1. [ ] F12 → Console tab
2. [ ] While doing all above tests
3. [ ] [ ] Should see NO red error messages
4. [ ] [ ] Warnings are OK
5. [ ] [ ] Should see logs like:
   - "Service Worker registered"
   - "Network status changed: online/offline"
   - "Successfully added score for game"

## Test 15: Offline with No Previous Cache
1. [ ] Clear localStorage completely
2. [ ] Clear browser cache
3. [ ] Close DevTools
4. [ ] Close and reopen browser
5. [ ] Go offline (network offline)
6. [ ] Try to open app
7. [ ] [ ] Homepage should show (cached)
8. [ ] Click on game link
9. [ ] [ ] Should show cached game OR "Game not found" gracefully
10. [ ] [ ] Should NOT crash

## Success Criteria

### Core Functionality ✅
- Scores queue when offline
- Scores persist in localStorage
- Scores sync automatically when online
- Game data is cached for offline access

### UI/UX ✅
- Online/offline badge shows correctly
- Pending score counter visible
- Pending badges distinguish from synced
- Clear messaging about offline status
- Sync button appears only when needed

### Reliability ✅
- Scores survive page refresh
- Multiple games handled correctly
- No console errors
- Graceful fallbacks
- Mobile layout works

### Performance ✅
- LocalStorage operations are fast
- No noticeable lag when syncing
- Cache expiration works correctly
- Service worker caching effective

---

## Test Environment Notes

**Browser:** Chrome/Edge 90+  
**OS:** Windows 10/11, macOS, iOS, Android  
**Network:** WiFi, 4G (using Network Throttling in DevTools)  
**Database:** SQL Server 2025 with sample data  
**Ports:** API 5022, App 5049  

---

**Test Completed:** _______________  
**Tester Name:** _______________  
**Notes:** _______________  
