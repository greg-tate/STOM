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
            Console.WriteLine($"[DEBUG] Fetching audit logs from: {query}");
            var result = await _httpClient.GetFromJsonAsync<List<AuditLog>>(query) ?? new();
            Console.WriteLine($"[DEBUG] Got {result.Count} audit logs");
            return result;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ERROR] GetAuditLogsAsync failed: {ex.Message}");
            Console.WriteLine($"[ERROR] StatusCode: {ex.StatusCode}");
            if (ex.InnerException != null)
                Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
            return new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetAuditLogsAsync unexpected error: {ex.Message}");
            return new();
        }
    }
}
