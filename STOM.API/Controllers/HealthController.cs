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
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        StomDbContext dbContext,
        DatabaseInitializationService dbInitService,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _dbInitService = dbInitService;
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
        var isHealthy = await _dbInitService.VerifyDatabaseHealthAsync(_dbContext);
        
        if (!isHealthy)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthStatus
                {
                    Status = "Unhealthy",
                    Message = "Database connection failed",
                    Timestamp = DateTime.UtcNow
                });
        }

        return Ok(new HealthStatus
        {
            Status = "Healthy",
            Message = "All systems operational",
            Timestamp = DateTime.UtcNow
        });
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
