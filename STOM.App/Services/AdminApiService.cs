using STOM.App.Models;

namespace STOM.App.Services;

public class AdminApiService
{
    private readonly HttpClient _httpClient;

    public AdminApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5022/api/admin/");
    }

    public async Task<Tournament?> CreateCompetitionAsync(CompetitionRequest req)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("competitions/create", req);
            return await response.Content.ReadFromJsonAsync<Tournament>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? since = null)
    {
        try
        {
            var query = since.HasValue ? $"audit-logs?since={since.Value:O}" : "audit-logs";
            return await _httpClient.GetFromJsonAsync<List<AuditLog>>(query) ?? new();
        }
        catch
        {
            return new();
        }
    }
}
