using STOM.App.Models;

namespace STOM.App.Services;

public class CacheService
{
    private const string GamesCacheKey = "games_all";
    private const string CacheDurationMinutes = "60";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly IndexedDbService _indexedDb;

    public CacheService(IndexedDbService indexedDb)
    {
        _indexedDb = indexedDb;
    }

    public async Task<List<Game>?> GetCachedGamesAsync()
    {
        try
        {
            var cachedGames = await _indexedDb.GetAsync<CachedItem<List<Game>>>("game_cache", GamesCacheKey);
            
            if (cachedGames != null && DateTime.UtcNow - cachedGames.CacheTime < CacheDuration)
            {
                return cachedGames.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading games cache: {ex.Message}");
        }

        return null;
    }

    public async Task CacheGamesAsync(List<Game> games)
    {
        try
        {
            var cachedItem = new CachedItem<List<Game>>
            {
                Data = games,
                CacheTime = DateTime.UtcNow
            };
            await _indexedDb.SetAsync("game_cache", GamesCacheKey, cachedItem);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error caching games: {ex.Message}");
        }
    }

    public async Task<Game?> GetCachedGameAsync(int gameId)
    {
        try
        {
            var cachedGame = await _indexedDb.GetAsync<CachedItem<Game>>("game_cache", $"game_{gameId}");
            
            if (cachedGame != null && DateTime.UtcNow - cachedGame.CacheTime < CacheDuration)
            {
                return cachedGame.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading game cache: {ex.Message}");
        }

        return null;
    }

    public async Task CacheGameAsync(Game game)
    {
        try
        {
            var cachedItem = new CachedItem<Game>
            {
                Data = game,
                CacheTime = DateTime.UtcNow
            };
            await _indexedDb.SetAsync("game_cache", $"game_{game.Id}", cachedItem);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error caching game: {ex.Message}");
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            await _indexedDb.ClearStoreAsync("game_cache");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }
}

public class CachedItem<T>
{
    public T? Data { get; set; }
    public DateTime CacheTime { get; set; }
}
