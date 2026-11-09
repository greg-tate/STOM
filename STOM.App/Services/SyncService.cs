using STOM.App.Models;

namespace STOM.App.Services;

public class SyncService
{
    private readonly ApiService _apiService;
    private readonly List<Score> _pendingScores = new();

    public SyncService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task SaveScoreOfflineAsync(int gameId, int value)
    {
        var score = new Score { GameId = gameId, Value = value };
        _pendingScores.Add(score);

        await TrySyncAsync();
    }

    public async Task TrySyncAsync()
    {
        var toRemove = new List<Score>();

        foreach (var score in _pendingScores)
        {
            try
            {
                var result = await _apiService.AddScoreAsync(score.GameId, score.Value);
                if (result != null)
                {
                    toRemove.Add(score);
                }
            }
            catch
            {
                break;
            }
        }

        foreach (var score in toRemove)
        {
            _pendingScores.Remove(score);
        }
    }

    public int GetPendingCount() => _pendingScores.Count;
}
