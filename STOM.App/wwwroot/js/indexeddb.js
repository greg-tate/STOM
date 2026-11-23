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
            console.error('IndexedDB open failed:', request.error);
            reject(request.error);
        };

        request.onsuccess = () => {
            db = request.result;
            console.log('IndexedDB initialized successfully');
            resolve();
        };

        request.onupgradeneeded = (event) => {
            const database = event.target.result;

            // Create pending_scores store
            if (!database.objectStoreNames.contains(STORES.PENDING_SCORES)) {
                const scoreStore = database.createObjectStore(STORES.PENDING_SCORES, { keyPath: 'id' });
                scoreStore.createIndex('gameId', 'gameId', { unique: false });
                scoreStore.createIndex('synced', 'synced', { unique: false });
            }

            // Create game_cache store
            if (!database.objectStoreNames.contains(STORES.GAME_CACHE)) {
                const cacheStore = database.createObjectStore(STORES.GAME_CACHE, { keyPath: 'id' });
                cacheStore.createIndex('cacheTime', 'cacheTime', { unique: false });
            }
        };
    });
}

export function setItem(storeName, key, value) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([storeName], 'readwrite');
        const store = transaction.objectStore(storeName);
        const request = store.put({ ...value, id: key, cacheTime: Date.now() });

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve();
    });
}

export function getItem(storeName, key) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([storeName], 'readonly');
        const store = transaction.objectStore(storeName);
        const request = store.get(key);

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);
    });
}

export function getAllItems(storeName) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([storeName], 'readonly');
        const store = transaction.objectStore(storeName);
        const request = store.getAll();

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);
    });
}

export function deleteItem(storeName, key) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([storeName], 'readwrite');
        const store = transaction.objectStore(storeName);
        const request = store.delete(key);

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve();
    });
}

export function clearStore(storeName) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([storeName], 'readwrite');
        const store = transaction.objectStore(storeName);
        const request = store.clear();

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve();
    });
}

export function getStoreName(type) {
    return STORES[type] || null;
}
