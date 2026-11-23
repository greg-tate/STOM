using Microsoft.JSInterop;

namespace STOM.App.Services;

public class IndexedDbService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public IndexedDbService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/indexeddb.js");
        await _module.InvokeVoidAsync("initializeDb");
    }

    public async Task SetAsync(string storeName, string key, object value)
    {
        if (_module == null) await InitializeAsync();
        await _module!.InvokeVoidAsync("setItem", storeName, key, value);
    }

    public async Task<T?> GetAsync<T>(string storeName, string key)
    {
        if (_module == null) await InitializeAsync();
        return await _module!.InvokeAsync<T>("getItem", storeName, key);
    }

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        if (_module == null) await InitializeAsync();
        return await _module!.InvokeAsync<List<T>>("getAllItems", storeName) ?? new();
    }

    public async Task DeleteAsync(string storeName, string key)
    {
        if (_module == null) await InitializeAsync();
        await _module!.InvokeVoidAsync("deleteItem", storeName, key);
    }

    public async Task ClearStoreAsync(string storeName)
    {
        if (_module == null) await InitializeAsync();
        await _module!.InvokeVoidAsync("clearStore", storeName);
    }
}
