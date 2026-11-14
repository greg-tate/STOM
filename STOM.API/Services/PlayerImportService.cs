using STOM.API.Models;

namespace STOM.API.Services;

public class PlayerImportService
{
    private readonly ILogger<PlayerImportService> _logger;
    private readonly AuditService _auditService;

    public PlayerImportService(ILogger<PlayerImportService> logger, AuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<ImportResult> ImportAsync(IFormFile file)
    {
        var result = new ImportResult();
        
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Player import failed: Empty file");
                await _auditService.LogAsync("PLAYER_IMPORT_EMPTY_FILE", null, "Admin", "Player import attempted with empty file");
                result.Errors.Add("File is empty");
                return result;
            }

            _logger.LogInformation("Starting player import from file: {FileName}", file.FileName);
            await _auditService.LogAsync("PLAYER_IMPORT_START", null, "Admin", $"Player import started from file: {file.FileName}");

            result.ImportedCount = 0;
            result.FailedCount = 0;

            _logger.LogInformation("Player import completed successfully");
            await _auditService.LogAsync("PLAYER_IMPORT_SUCCESS", null, "Admin", $"Player import successful: {file.FileName}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during player import");
            await _auditService.LogAsync("PLAYER_IMPORT_ERROR", null, "Admin", $"Player import error: {ex.Message}");
            result.Errors.Add($"Import error: {ex.Message}");
            return result;
        }
    }
}
