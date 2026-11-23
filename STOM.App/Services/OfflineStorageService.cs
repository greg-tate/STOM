using STOM.App.Models;

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
    private const string StoreName = "pending_scores";
    private List<PendingScore> _pendingScores = new();
    private readonly IndexedDbService _indexedDb;

    public OfflineStorageService(IndexedDbService indexedDb)
    {
        _indexedDb = indexedDb;
    }

    public async Task InitializeAsync()
    {
        await LoadPendingScoresAsync();
    }

    private async Task LoadPendingScoresAsync()
    {
        try
        {
            _pendingScores = await _indexedDb.GetAllAsync<PendingScore>(StoreName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading pending scores from IndexedDB: {ex.Message}");
            _pendingScores = new();
        }
    }

    public async Task AddPendingScoreAsync(int gameId, int value)
    {
        var score = new PendingScore { GameId = gameId, Value = value };
        _pendingScores.Add(score);
        
        try
        {
            await _indexedDb.SetAsync(StoreName, score.Id, score);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving pending score to IndexedDB: {ex.Message}");
        }
    }

    public async Task MarkSyncedAsync(string scoreId)
    {
        var score = _pendingScores.FirstOrDefault(s => s.Id == scoreId);
        if (score != null)
        {
            score.Synced = true;
            
            try
            {
                await _indexedDb.SetAsync(StoreName, scoreId, score);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking score as synced: {ex.Message}");
            }
        }
    }

    public async Task RemoveSyncedAsync(string scoreId)
    {
        _pendingScores.RemoveAll(s => s.Id == scoreId && s.Synced);
        
        try
        {
            await _indexedDb.DeleteAsync(StoreName, scoreId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing synced score: {ex.Message}");
        }
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
        
        try
        {
            await _indexedDb.ClearStoreAsync(StoreName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing pending scores: {ex.Message}");
        }
    }
}
