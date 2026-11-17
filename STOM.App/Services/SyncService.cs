using STOM.App.Models;

namespace STOM.App.Services;

public class SyncService
{
    private readonly ApiService _apiService;
    private readonly OfflineStorageService _offlineStorageService;
    private readonly OfflineCapabilityService _offlineCapabilityService;
    private readonly ILogger<SyncService> _logger;

    public event EventHandler<string>? SyncStatusChanged;

    public SyncService(
        ApiService apiService,
        OfflineStorageService offlineStorageService,
        OfflineCapabilityService offlineCapabilityService,
        ILogger<SyncService> logger)
    {
        _apiService = apiService;
        _offlineStorageService = offlineStorageService;
        _offlineCapabilityService = offlineCapabilityService;
        _logger = logger;

        _apiService.SyncStatusChanged += (s, msg) => SyncStatusChanged?.Invoke(this, msg);
    }

    public async Task InitializeAsync()
    {
        await _offlineStorageService.InitializeAsync();
        _offlineCapabilityService.OnlineStatusChanged += async (s, isOnline) =>
        {
            if (isOnline)
            {
                _logger.LogInformation("Network connection restored");
                SyncStatusChanged?.Invoke(this, "Connection restored, syncing...");
                await TrySyncAsync();
            }
            else
            {
                _logger.LogInformation("Network connection lost");
                SyncStatusChanged?.Invoke(this, "Offline mode");
            }
        };

        var pendingCount = GetPendingCount();
        if (pendingCount > 0)
        {
            _logger.LogInformation("Found {Count} pending scores from previous session", pendingCount);
        }
    }

    public async Task SaveScoreOfflineAsync(int gameId, int value)
    {
        await _offlineStorageService.AddPendingScoreAsync(gameId, value);
        _logger.LogInformation("Score queued for game {GameId}", gameId);
    }

    public async Task TrySyncAsync()
    {
        if (!_offlineCapabilityService.IsOnline)
        {
            _logger.LogInformation("Still offline, cannot sync");
            return;
        }

        var success = await _apiService.SyncPendingScoresAsync();

        if (success && GetPendingCount() == 0)
        {
            _logger.LogInformation("All scores synced successfully");
        }
    }

    public int GetPendingCount() => _offlineStorageService.GetPendingCount();

    public List<PendingScore> GetPendingScores() => _offlineStorageService.GetPendingScores();

    public async Task ClearOfflineDataAsync()
    {
        await _offlineStorageService.ClearAllAsync();
        _logger.LogInformation("Offline data cleared");
    }
}
