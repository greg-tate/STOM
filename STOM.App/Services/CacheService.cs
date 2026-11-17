using STOM.App.Models;
using System.Text.Json;
using Microsoft.JSInterop;

namespace STOM.App.Services;

public class CacheService
{
    private const string GamesCacheKey = "stom_games_cache";
    private const string GameCacheKeyPrefix = "stom_game_";
    private const string CacheTimestampPrefix = "stom_cache_time_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private IJSRuntime _jsRuntime = default!;

    public CacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<Game>?> GetCachedGamesAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", GamesCacheKey);
            var timeJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"{CacheTimestampPrefix}games");

            if (!string.IsNullOrEmpty(json) && !string.IsNullOrEmpty(timeJson) && DateTime.TryParse(timeJson, out var cacheTime))
            {
                if (DateTime.UtcNow - cacheTime < CacheDuration)
                {
                    return JsonSerializer.Deserialize<List<Game>>(json);
                }
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
            var json = JsonSerializer.Serialize(games);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GamesCacheKey, json);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{CacheTimestampPrefix}games", DateTime.UtcNow.ToString("O"));
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
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"{GameCacheKeyPrefix}{gameId}");
            var timeJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"{CacheTimestampPrefix}game_{gameId}");

            if (!string.IsNullOrEmpty(json) && !string.IsNullOrEmpty(timeJson) && DateTime.TryParse(timeJson, out var cacheTime))
            {
                if (DateTime.UtcNow - cacheTime < CacheDuration)
                {
                    return JsonSerializer.Deserialize<Game>(json);
                }
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
            var json = JsonSerializer.Serialize(game);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{GameCacheKeyPrefix}{game.Id}", json);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"{CacheTimestampPrefix}game_{game.Id}", DateTime.UtcNow.ToString("O"));
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
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", GamesCacheKey);
            var keys = await _jsRuntime.InvokeAsync<string[]>("eval", "Object.keys(localStorage).filter(k => k.startsWith('stom_cache_time_'))");
            foreach (var key in keys)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }
}
