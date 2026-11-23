using STOM.App.Models;
using Microsoft.JSInterop;

namespace STOM.App.Services;

public class OfflineCapabilityService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isOnline = true;

    public event EventHandler<bool>? OnlineStatusChanged;

    public OfflineCapabilityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _isOnline = await _jsRuntime.InvokeAsync<bool>("isOnline");
            await _jsRuntime.InvokeVoidAsync("window.setupOnlineStatusListener", DotNetObjectReference.Create(this));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing offline capability: {ex.Message}");
        }
    }

    [JSInvokable]
    public void OnOnlineStatusChanged(bool isOnline)
    {
        _isOnline = isOnline;
        OnlineStatusChanged?.Invoke(this, isOnline);
    }

    public bool IsOnline => _isOnline;

    public bool IsOffline => !_isOnline;

    public async Task<bool> CheckNetworkAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("isOnline");
        }
        catch
        {
            return false;
        }
    }
}
