using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STOM.API.Data;
using STOM.API.Models;
using STOM.API.Services;

namespace STOM.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly StomDbContext _context;
    private readonly AuditService _auditService;
    private readonly BracketGenerator _bracketGenerator;
    private readonly PlayerImportService _playerImportService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        StomDbContext context,
        AuditService auditService,
        BracketGenerator bracketGenerator,
        PlayerImportService playerImportService,
        ILogger<AdminController> logger)
    {
        _context = context;
        _auditService = auditService;
        _bracketGenerator = bracketGenerator;
        _playerImportService = playerImportService;
        _logger = logger;
    }

    [HttpPost("competitions/create")]
    public async Task<ActionResult<Tournament>> CreateCompetition(CompetitionRequest req)
    {
        var competition = new Tournament { Name = req.Name, Date = req.Date, Status = "Planning" };
        _context.Tournaments.Add(competition);
        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "Unknown";
        await _auditService.LogAsync("Competition created", competition.Id, userId, $"Competition '{req.Name}'");

        return CreatedAtAction(nameof(CreateCompetition), competition);
    }

    [HttpPost("competitions/{id}/brackets")]
    public async Task<ActionResult<List<Tournament>>> GenerateBrackets(int id)
    {
        var brackets = await _bracketGenerator.GenerateAsync(id);

        var userId = User.Identity?.Name ?? "Unknown";
        await _auditService.LogAsync("Tournament brackets generated", id, userId, $"Brackets for competition {id}");

        return Ok(brackets);
    }

    [HttpPost("players/import-bulk")]
    public async Task<ActionResult<ImportResult>> ImportPlayers(IFormFile file)
    {
        var result = await _playerImportService.ImportAsync(file);

        var userId = User.Identity?.Name ?? "Unknown";
        await _auditService.LogAsync("Bulk player import", null, userId, $"Imported {result.ImportedCount} players");

        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<List<AuditLog>>> GetAuditLogs([FromQuery] DateTime? since)
    {
        try
        {
            _logger.LogInformation("GetAuditLogs called with since={Since}", since);
            
            if (!await _context.Database.CanConnectAsync())
            {
                _logger.LogError("Cannot connect to database");
                return StatusCode(500, new { error = "Database connection failed" });
            }

            var query = _context.AuditLogs.AsQueryable();
            _logger.LogInformation("AuditLogs table accessible");

            if (since.HasValue)
                query = query.Where(a => a.Timestamp >= since.Value);

            var logs = await query.OrderByDescending(a => a.Timestamp).ToListAsync();
            _logger.LogInformation("Retrieved {Count} audit logs", logs.Count);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs: {Message}", ex.Message);
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost("scores/{id}/correction")]
    public async Task<ActionResult<Score>> CorrectScore(int id, AddScoreRequest request)
    {
        var score = await _context.Scores.FindAsync(id);
        if (score == null)
            return NotFound("Score not found");

        var oldValue = score.Value;
        score.Value = request.Value;
        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "Unknown";
        await _auditService.LogAsync("Score corrected", id, userId, $"Changed from {oldValue} to {request.Value}");

        return Ok(score);
    }
}
