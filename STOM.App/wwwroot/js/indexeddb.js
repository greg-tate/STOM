// IndexedDB wrapper for Blazor

const DB_NAME = 'STOM';
const DB_VERSION = 1;
const STORES = {
    PENDING_SCORES: 'pending_scores',
    GAME_CACHE: 'game_cache'
};

let db = null;

export function initializeDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onerror = () => {
            console.error('[IDB] Open failed:', request.error);
            reject(request.error);
        };

        request.onsuccess = () => {
            db = request.result;
            console.log('[IDB] Database opened successfully, version:', db.version);
            console.log('[IDB] Object stores:', Array.from(db.objectStoreNames));
            resolve();
        };

        request.onupgradeneeded = (event) => {
            const database = event.target.result;
            console.log('[IDB] Upgrade needed, old version:', event.oldVersion, 'new version:', event.newVersion);

            // Create pending_scores store
            if (!database.objectStoreNames.contains(STORES.PENDING_SCORES)) {
                const scoreStore = database.createObjectStore(STORES.PENDING_SCORES, { keyPath: 'id' });
                scoreStore.createIndex('gameId', 'gameId', { unique: false });
                scoreStore.createIndex('synced', 'synced', { unique: false });
                console.log('[IDB] Created store:', STORES.PENDING_SCORES);
            }

            // Create game_cache store
            if (!database.objectStoreNames.contains(STORES.GAME_CACHE)) {
                const cacheStore = database.createObjectStore(STORES.GAME_CACHE, { keyPath: 'id' });
                cacheStore.createIndex('cacheTime', 'cacheTime', { unique: false });
                console.log('[IDB] Created store:', STORES.GAME_CACHE);
            }
        };
    });
}

export function setItem(storeName, key, value) {
    return new Promise((resolve, reject) => {
        if (!db) {
            console.error('[IDB] Database not initialized');
            reject(new Error('Database not initialized'));
            return;
        }

        try {
            const transaction = db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            
            const dataToStore = { ...value, id: key, cacheTime: Date.now() };
            console.log('[IDB] Writing to', storeName, 'key:', key, 'data:', dataToStore);
            
            const request = store.put(dataToStore);

            request.onerror = () => {
                console.error('[IDB] Write failed:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                console.log('[IDB] Write successful:', storeName, key);
                resolve();
            };

            transaction.onerror = () => {
                console.error('[IDB] Transaction error:', transaction.error);
                reject(transaction.error);
            };
        } catch (err) {
            console.error('[IDB] Exception in setItem:', err);
            reject(err);
        }
    });
}

export function getItem(storeName, key) {
    return new Promise((resolve, reject) => {
        if (!db) {
            console.error('[IDB] Database not initialized');
            reject(new Error('Database not initialized'));
            return;
        }

        try {
            const transaction = db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.get(key);

            request.onerror = () => {
                console.error('[IDB] Read failed:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                console.log('[IDB] Read successful:', storeName, key, 'result:', request.result);
                resolve(request.result);
            };
        } catch (err) {
            console.error('[IDB] Exception in getItem:', err);
            reject(err);
        }
    });
}

export function getAllItems(storeName) {
    return new Promise((resolve, reject) => {
        if (!db) {
            console.error('[IDB] Database not initialized');
            reject(new Error('Database not initialized'));
            return;
        }

        try {
            const transaction = db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.getAll();

            request.onerror = () => {
                console.error('[IDB] GetAll failed:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                console.log('[IDB] GetAll successful:', storeName, 'count:', request.result.length, 'results:', request.result);
                resolve(request.result);
            };
        } catch (err) {
            console.error('[IDB] Exception in getAllItems:', err);
            reject(err);
        }
    });
}

export function deleteItem(storeName, key) {
    return new Promise((resolve, reject) => {
        if (!db) {
            console.error('[IDB] Database not initialized');
            reject(new Error('Database not initialized'));
            return;
        }

        try {
            const transaction = db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.delete(key);

            request.onerror = () => {
                console.error('[IDB] Delete failed:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                console.log('[IDB] Delete successful:', storeName, key);
                resolve();
            };
        } catch (err) {
            console.error('[IDB] Exception in deleteItem:', err);
            reject(err);
        }
    });
}

export function clearStore(storeName) {
    return new Promise((resolve, reject) => {
        if (!db) {
            console.error('[IDB] Database not initialized');
            reject(new Error('Database not initialized'));
            return;
        }

        try {
            const transaction = db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.clear();

            request.onerror = () => {
                console.error('[IDB] Clear failed:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                console.log('[IDB] Clear successful:', storeName);
                resolve();
            };
        } catch (err) {
            console.error('[IDB] Exception in clearStore:', err);
            reject(err);
        }
    });
}

export function getStoreName(type) {
    return STORES[type] || null;
}

