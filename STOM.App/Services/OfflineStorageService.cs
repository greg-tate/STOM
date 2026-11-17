using STOM.App.Models;
using System.Text.Json;
using Microsoft.JSInterop;

namespace STOM.App.Services;

public class PendingScore
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int GameId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Synced { get; set; }
}

public class OfflineStorageService
{
    private const string StorageKey = "stom_pending_scores";
    private List<PendingScore> _pendingScores = new();

    public async Task InitializeAsync()
    {
        await LoadPendingScoresAsync();
    }

    private async Task LoadPendingScoresAsync()
    {
        try
        {
            var json = await JS.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                _pendingScores = JsonSerializer.Deserialize<List<PendingScore>>(json) ?? new();
            }
        }
        catch
        {
            _pendingScores = new();
        }
    }

    private async Task SavePendingScoresAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_pendingScores);
            await JS.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving pending scores: {ex.Message}");
        }
    }

    public async Task AddPendingScoreAsync(int gameId, int value)
    {
        var score = new PendingScore { GameId = gameId, Value = value };
        _pendingScores.Add(score);
        await SavePendingScoresAsync();
    }

    public async Task MarkSyncedAsync(string scoreId)
    {
        var score = _pendingScores.FirstOrDefault(s => s.Id == scoreId);
        if (score != null)
        {
            score.Synced = true;
            await SavePendingScoresAsync();
        }
    }

    public async Task RemoveSyncedAsync(string scoreId)
    {
        _pendingScores.RemoveAll(s => s.Id == scoreId && s.Synced);
        await SavePendingScoresAsync();
    }

    public List<PendingScore> GetPendingScores()
    {
        return _pendingScores.Where(s => !s.Synced).ToList();
    }

    public List<PendingScore> GetAllPendingScores()
    {
        return new List<PendingScore>(_pendingScores);
    }

    public int GetPendingCount()
    {
        return _pendingScores.Count(s => !s.Synced);
    }

    public async Task ClearAllAsync()
    {
        _pendingScores.Clear();
        await SavePendingScoresAsync();
    }

    private static IJSRuntime JS { get; set; } = default!;

    public static void SetJsRuntime(IJSRuntime jsRuntime)
    {
        JS = jsRuntime;
    }
}
