using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STOM.API.Data;
using STOM.API.Models;
using STOM.API.Services;

namespace STOM.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly StomDbContext _context;
    private readonly AuditService _auditService;
    private readonly BracketGenerator _bracketGenerator;
    private readonly PlayerImportService _playerImportService;

    public AdminController(
        StomDbContext context,
        AuditService auditService,
        BracketGenerator bracketGenerator,
        PlayerImportService playerImportService)
    {
        _context = context;
        _auditService = auditService;
        _bracketGenerator = bracketGenerator;
        _playerImportService = playerImportService;
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
        var query = _context.AuditLogs.AsQueryable();
        if (since.HasValue)
            query = query.Where(a => a.Timestamp >= since.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
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
