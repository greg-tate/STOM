using Microsoft.JSInterop;

namespace STOM.App.Services;

public class IndexedDbService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private readonly ILogger<IndexedDbService> _logger;

    public IndexedDbService(IJSRuntime jsRuntime, ILogger<IndexedDbService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Loading indexeddb.js module...");
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/indexeddb.js");
            
            _logger.LogInformation("Calling initializeDb()...");
            await _module.InvokeVoidAsync("initializeDb");
            
            _logger.LogInformation("IndexedDB initialized successfully");
            Console.WriteLine("[DEBUG] IndexedDB initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize IndexedDB");
            Console.WriteLine($"[ERROR] Failed to initialize IndexedDB: {ex.Message}");
            throw;
        }
    }

    public async Task SetAsync(string storeName, string key, object value)
    {
        if (_module == null)
        {
            _logger.LogWarning("Module not initialized, initializing now...");
            await InitializeAsync();
        }

        try
        {
            _logger.LogInformation("Setting {StoreName}[{Key}] = {@Value}", storeName, key, value);
            await _module!.InvokeVoidAsync("setItem", storeName, key, value);
            Console.WriteLine($"[DEBUG] Stored {storeName}[{key}]");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set item in {StoreName}", storeName);
            Console.WriteLine($"[ERROR] Failed to set {storeName}[{key}]: {ex.Message}");
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string storeName, string key)
    {
        if (_module == null)
        {
            _logger.LogWarning("Module not initialized, initializing now...");
            await InitializeAsync();
        }

        try
        {
            var result = await _module!.InvokeAsync<T>("getItem", storeName, key);
            _logger.LogInformation("Retrieved {StoreName}[{Key}] = {@Result}", storeName, key, result);
            Console.WriteLine($"[DEBUG] Retrieved {storeName}[{key}]: {result != null}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get item from {StoreName}", storeName);
            Console.WriteLine($"[ERROR] Failed to get {storeName}[{key}]: {ex.Message}");
            return default;
        }
    }

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        if (_module == null)
        {
            _logger.LogWarning("Module not initialized, initializing now...");
            await InitializeAsync();
        }

        try
        {
            var result = await _module!.InvokeAsync<List<T>>("getAllItems", storeName);
            _logger.LogInformation("Retrieved all items from {StoreName}: {Count} items", storeName, result?.Count ?? 0);
            Console.WriteLine($"[DEBUG] Retrieved {storeName}: {result?.Count ?? 0} items");
            return result ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all items from {StoreName}", storeName);
            Console.WriteLine($"[ERROR] Failed to getAll from {storeName}: {ex.Message}");
            return new();
        }
    }

    public async Task DeleteAsync(string storeName, string key)
    {
        if (_module == null)
        {
            _logger.LogWarning("Module not initialized, initializing now...");
            await InitializeAsync();
        }

        try
        {
            await _module!.InvokeVoidAsync("deleteItem", storeName, key);
            _logger.LogInformation("Deleted {StoreName}[{Key}]", storeName, key);
            Console.WriteLine($"[DEBUG] Deleted {storeName}[{key}]");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete item from {StoreName}", storeName);
            Console.WriteLine($"[ERROR] Failed to delete {storeName}[{key}]: {ex.Message}");
        }
    }

    public async Task ClearStoreAsync(string storeName)
    {
        if (_module == null)
        {
            _logger.LogWarning("Module not initialized, initializing now...");
            await InitializeAsync();
        }

        try
        {
            await _module!.InvokeVoidAsync("clearStore", storeName);
            _logger.LogInformation("Cleared {StoreName}", storeName);
            Console.WriteLine($"[DEBUG] Cleared {storeName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear {StoreName}", storeName);
            Console.WriteLine($"[ERROR] Failed to clear {storeName}: {ex.Message}");
        }
    }
}
