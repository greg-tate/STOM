// Register Service Worker (deferred to avoid blocking main thread)
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        // Use requestIdleCallback if available, otherwise defer with setTimeout
        if ('requestIdleCallback' in window) {
            requestIdleCallback(() => {
                navigator.serviceWorker.register('/service-worker.js')
                    .then(reg => {
                        console.log('Service Worker registered successfully:', reg);
                    })
                    .catch(err => {
                        console.log('Service Worker registration failed:', err);
                    });
            });
        } else {
            setTimeout(() => {
                navigator.serviceWorker.register('/service-worker.js')
                    .then(reg => {
                        console.log('Service Worker registered successfully:', reg);
                    })
                    .catch(err => {
                        console.log('Service Worker registration failed:', err);
                    });
            }, 0);
        }
    });
}

// Setup online/offline status listener
window.setupOnlineStatusListener = function (dotnetRef) {
    const updateStatus = () => {
        const isOnline = navigator.onLine;
        console.log('Network status changed:', isOnline ? 'Online' : 'Offline');
        dotnetRef.invokeMethodAsync('OnOnlineStatusChanged', isOnline);
    };

    window.addEventListener('online', updateStatus);
    window.addEventListener('offline', updateStatus);

    // Initial status
    updateStatus();
};

// Persist to localStorage
window.persistData = function (key, data) {
    try {
        localStorage.setItem(key, JSON.stringify(data));
        return true;
    } catch (e) {
        console.error('Error persisting data:', e);
        return false;
    }
};

// Retrieve from localStorage
window.retrieveData = function (key) {
    try {
        const data = localStorage.getItem(key);
        return data ? JSON.parse(data) : null;
    } catch (e) {
        console.error('Error retrieving data:', e);
        return null;
    }
};

// Clear localStorage
window.clearLocalStorage = function (key) {
    try {
        if (key) {
            localStorage.removeItem(key);
        } else {
            localStorage.clear();
        }
        return true;
    } catch (e) {
        console.error('Error clearing storage:', e);
        return false;
    }
};

// Get all keys in localStorage
window.getLocalStorageKeys = function () {
    try {
        return Object.keys(localStorage);
    } catch (e) {
        console.error('Error getting keys:', e);
        return [];
    }
};

// Check if device is online
window.isOnline = function () {
    return navigator.onLine;
};

// Listen for page visibility changes to trigger sync
document.addEventListener('visibilitychange', () => {
    if (!document.hidden && navigator.onLine) {
        console.log('Page became visible and online, triggering sync');
        // This will be handled by the UI
    }
});
