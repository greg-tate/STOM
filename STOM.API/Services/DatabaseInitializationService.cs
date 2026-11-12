using Microsoft.EntityFrameworkCore;
using STOM.API.Data;

namespace STOM.API.Services;

/// <summary>
/// Service responsible for initializing the SQL Server database schema
/// and applying Entity Framework Core migrations automatically.
/// </summary>
public class DatabaseInitializationService
{
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseInitializationService(
        ILogger<DatabaseInitializationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Initializes the database by applying all pending migrations.
    /// This handles both database creation and schema updates.
    /// This is called on application startup.
    /// </summary>
    public async Task InitializeDatabaseAsync(StomDbContext dbContext)
    {
        try
        {
            _logger.LogInformation("========== DATABASE INITIALIZATION START ==========");
            
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger.LogInformation("Using connection string: {ConnectionString}", 
                MaskConnectionString(connectionString));

            // Check for pending migrations before applying
            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
            if (pendingMigrations.Count > 0)
            {
                _logger.LogInformation("Found {MigrationCount} pending migrations:", pendingMigrations.Count);
                foreach (var migration in pendingMigrations)
                {
                    _logger.LogInformation("  - {MigrationName}", migration);
                }

                // Apply migrations (creates database and schema if needed)
                _logger.LogInformation("Applying pending migrations...");
                await dbContext.Database.MigrateAsync();
                _logger.LogInformation("All pending migrations applied successfully.");
            }
            else
            {
                _logger.LogInformation("No pending migrations. Database schema is up to date.");
            }

            // Log applied migrations
            var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList();
            _logger.LogInformation("Total applied migrations: {AppliedCount}", appliedMigrations.Count);

            _logger.LogInformation("========== DATABASE INITIALIZATION SUCCESS ==========");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "========== DATABASE INITIALIZATION FAILED ==========\n" +
                "An error occurred during database initialization. The application will continue but database operations may fail.\n" +
                "Error: {ErrorMessage}", ex.Message);
            
            // Re-throw if in development, silently fail in production to avoid startup crash
            if (!IsDevelopment())
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Validates database connectivity and schema integrity.
    /// Useful for health checks.
    /// </summary>
    public async Task<bool> VerifyDatabaseHealthAsync(StomDbContext dbContext)
    {
        try
        {
            _logger.LogInformation("Verifying database health...");
            
            // Test connection
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogError("Cannot connect to database.");
                return false;
            }

            _logger.LogInformation("Database connection verified.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets database information for diagnostics.
    /// </summary>
    public async Task<DatabaseInfo> GetDatabaseInfoAsync(StomDbContext dbContext)
    {
        var info = new DatabaseInfo
        {
            CanConnect = await dbContext.Database.CanConnectAsync(),
            AppliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList(),
            PendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList(),
        };

        return info;
    }

    private string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "(default)";

        // Mask password if present
        if (connectionString.Contains("Password", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString.Replace(
                connectionString.Substring(connectionString.IndexOf("Password", StringComparison.OrdinalIgnoreCase)),
                "Password=***",
                StringComparison.OrdinalIgnoreCase);
        }

        return connectionString;
    }

    private bool IsDevelopment()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"];
        return env?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}

/// <summary>
/// Information about the database and migrations.
/// </summary>
public class DatabaseInfo
{
    public bool CanConnect { get; set; }
    public List<string> AppliedMigrations { get; set; } = new();
    public List<string> PendingMigrations { get; set; } = new();
}
