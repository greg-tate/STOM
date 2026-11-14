using STOM.App.Models;

namespace STOM.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Game>>("games") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all games");
            return new();
        }
    }

    public async Task<Game?> GetGameAsync(int gameId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Game>($"games/{gameId}");
            _logger.LogInformation("Successfully fetched game {GameId}", gameId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching game {GameId}", gameId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding score for game {GameId}", gameId);
            return null;
        }
    }
}
