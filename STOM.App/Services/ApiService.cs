using STOM.App.Models;

namespace STOM.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly CacheService _cacheService;
    private readonly OfflineStorageService _offlineStorageService;
    private readonly OfflineCapabilityService _offlineCapabilityService;

    public event EventHandler<string>? SyncStatusChanged;

    public ApiService(
        HttpClient httpClient,
        ILogger<ApiService> logger,
        CacheService cacheService,
        OfflineStorageService offlineStorageService,
        OfflineCapabilityService offlineCapabilityService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cacheService = cacheService;
        _offlineStorageService = offlineStorageService;
        _offlineCapabilityService = offlineCapabilityService;
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        try
        {
            var games = await _httpClient.GetFromJsonAsync<List<Game>>("games") ?? new();
            await _cacheService.CacheGamesAsync(games);
            return games;
        }
        catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("Failed to fetch") == true)
        {
            _logger.LogInformation("Offline: returning cached games");
            var cached = await _cacheService.GetCachedGamesAsync();
            return cached ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all games");

            if (_offlineCapabilityService.IsOffline)
            {
                _logger.LogInformation("Offline: returning cached games");
                var cached = await _cacheService.GetCachedGamesAsync();
                if (cached != null)
                {
                    return cached;
                }
            }

            return new();
        }
    }

    public async Task<Game?> GetGameAsync(int gameId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Game>($"games/{gameId}");
            if (result != null)
            {
                await _cacheService.CacheGameAsync(result);
            }
            _logger.LogInformation("Successfully fetched game {GameId}", gameId);
            return result;
        }
        catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("Failed to fetch") == true)
        {
            _logger.LogInformation("Offline: returning cached game {GameId}", gameId);
            var cached = await _cacheService.GetCachedGameAsync(gameId);
            return cached;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching game {GameId}", gameId);

            if (_offlineCapabilityService.IsOffline)
            {
                _logger.LogInformation("Offline: returning cached game {GameId}", gameId);
                var cached = await _cacheService.GetCachedGameAsync(gameId);
                if (cached != null)
                {
                    return cached;
                }
            }

            return null;
        }
    }

    public async Task<Score?> AddScoreAsync(int gameId, int value)
    {
        try
        {
            var request = new AddScoreRequest { Value = value };
            var response = await _httpClient.PostAsJsonAsync($"games/{gameId}/scores", request);
            var result = await response.Content.ReadFromJsonAsync<Score>();
            _logger.LogInformation("Successfully added score for game {GameId}", gameId);
            return result;
        }
        catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("Failed to fetch") == true)
        {
            _logger.LogError(ex, "Network error adding score for game {GameId} - treating as offline", gameId);
            Console.WriteLine($"[DEBUG] Network error detected - queuing score offline");
            
            await _offlineStorageService.AddPendingScoreAsync(gameId, value);
            SyncStatusChanged?.Invoke(this, $"Score queued (pending: {_offlineStorageService.GetPendingCount()})");

            return new Score
            {
                GameId = gameId,
                Value = value,
                CreatedAt = DateTime.UtcNow,
                IsPending = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding score for game {GameId}", gameId);

            if (_offlineCapabilityService.IsOffline)
            {
                _logger.LogInformation("Offline: queueing score for game {GameId}", gameId);
                await _offlineStorageService.AddPendingScoreAsync(gameId, value);
                SyncStatusChanged?.Invoke(this, $"Score queued (pending: {_offlineStorageService.GetPendingCount()})");

                return new Score
                {
                    GameId = gameId,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    IsPending = true
                };
            }

            return null;
        }
    }

    public async Task<bool> SyncPendingScoresAsync()
    {
        var pendingScores = _offlineStorageService.GetPendingScores();

        if (pendingScores.Count == 0)
        {
            _logger.LogInformation("No pending scores to sync");
            return true;
        }

        _logger.LogInformation("Syncing {Count} pending scores", pendingScores.Count);
        SyncStatusChanged?.Invoke(this, $"Syncing {pendingScores.Count} pending scores...");

        var synced = 0;
        foreach (var pending in pendingScores)
        {
            try
            {
                var request = new AddScoreRequest { Value = pending.Value };
                var response = await _httpClient.PostAsJsonAsync($"games/{pending.GameId}/scores", request);

                if (response.IsSuccessStatusCode)
                {
                    await _offlineStorageService.MarkSyncedAsync(pending.Id);
                    synced++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing score {ScoreId}", pending.Id);
                break;
            }
        }

        await Task.Delay(500);
        var remaining = _offlineStorageService.GetPendingCount();
        if (remaining == 0)
        {
            SyncStatusChanged?.Invoke(this, "All scores synced!");
            _logger.LogInformation("Successfully synced {Count} scores", synced);
            return true;
        }
        else
        {
            SyncStatusChanged?.Invoke(this, $"Synced {synced}, {remaining} pending");
            return false;
        }
    }

    public int GetPendingScoreCount()
    {
        return _offlineStorageService.GetPendingCount();
    }
}
