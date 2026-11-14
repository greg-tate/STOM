using Microsoft.AspNetCore.Mvc;
using STOM.API.Data;
using STOM.API.Services;

namespace STOM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly StomDbContext _dbContext;
    private readonly DatabaseInitializationService _dbInitService;
    private readonly AuditService _auditService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        StomDbContext dbContext,
        DatabaseInitializationService dbInitService,
        AuditService auditService,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _dbInitService = dbInitService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for monitoring application status.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthStatus>> GetHealthStatus()
    {
        try
        {
            var isHealthy = await _dbInitService.VerifyDatabaseHealthAsync(_dbContext);
            
            if (!isHealthy)
            {
                _logger.LogWarning("Health check failed: Database connection issue");
                await _auditService.LogAsync("HEALTH_CHECK_FAILED", null, "System", "Database connection failed during health check");
                
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new HealthStatus
                    {
                        Status = "Unhealthy",
                        Message = "Database connection failed",
                        Timestamp = DateTime.UtcNow
                    });
            }

            _logger.LogInformation("Health check passed");
            await _auditService.LogAsync("HEALTH_CHECK_PASSED", null, "System", "All systems operational");
            
            return Ok(new HealthStatus
            {
                Status = "Healthy",
                Message = "All systems operational",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            await _auditService.LogAsync("HEALTH_CHECK_ERROR", null, "System", $"Health check error: {ex.Message}");
            
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthStatus
                {
                    Status = "Error",
                    Message = "Health check failed",
                    Timestamp = DateTime.UtcNow
                });
        }
    }

    /// <summary>
    /// Detailed database diagnostics including migration information.
    /// </summary>
    [HttpGet("database")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DatabaseDiagnostics>> GetDatabaseDiagnostics()
    {
        try
        {
            var dbInfo = await _dbInitService.GetDatabaseInfoAsync(_dbContext);
            
            _logger.LogInformation("Retrieved database diagnostics: {AppliedCount} applied, {PendingCount} pending migrations",
                dbInfo.AppliedMigrations.Count, dbInfo.PendingMigrations.Count);
            await _auditService.LogAsync("DATABASE_DIAGNOSTICS_RETRIEVED", null, "System",
                $"Retrieved diagnostics: {dbInfo.AppliedMigrations.Count} applied migrations, {dbInfo.PendingMigrations.Count} pending");
            
            return Ok(new DatabaseDiagnostics
            {
                CanConnect = dbInfo.CanConnect,
                AppliedMigrationsCount = dbInfo.AppliedMigrations.Count,
                AppliedMigrations = dbInfo.AppliedMigrations,
                PendingMigrationsCount = dbInfo.PendingMigrations.Count,
                PendingMigrations = dbInfo.PendingMigrations,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving database diagnostics");
            await _auditService.LogAsync("DATABASE_DIAGNOSTICS_ERROR", null, "System", $"Failed to retrieve diagnostics: {ex.Message}");
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve database diagnostics", message = ex.Message });
        }
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DatabaseDiagnostics
{
    public bool CanConnect { get; set; }
    public int AppliedMigrationsCount { get; set; }
    public List<string> AppliedMigrations { get; set; } = new();
    public int PendingMigrationsCount { get; set; }
    public List<string> PendingMigrations { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
