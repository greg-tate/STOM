using STOM.App.Models;

namespace STOM.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5022/api/");
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Game>>("games") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<Game?> GetGameAsync(int gameId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Game>($"games/{gameId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<Score?> AddScoreAsync(int gameId, int value)
    {
        try
        {
            var request = new AddScoreRequest { Value = value };
            var response = await _httpClient.PostAsJsonAsync($"games/{gameId}/scores", request);
            return await response.Content.ReadFromJsonAsync<Score>();
        }
        catch
        {
            return null;
        }
    }
}
