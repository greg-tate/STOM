using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STOM.API.Data;
using STOM.API.Models;
using STOM.API.Services;

namespace STOM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly StomDbContext _context;
    private readonly AuditService _auditService;

    public GamesController(StomDbContext context, AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetAllGames()
    {
        return await _context.Games.Include(g => g.Scores).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(int id)
    {
        var game = await _context.Games
            .Include(g => g.Scores)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
            return NotFound();

        return Ok(game);
    }

    [HttpPost("{id}/scores")]
    public async Task<ActionResult<Score>> AddScore(int id, AddScoreRequest request)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null)
            return NotFound("Game not found");

        var score = new Score { GameId = id, Value = request.Value, CreatedAt = DateTime.UtcNow };
        _context.Scores.Add(score);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Score added", score.Id, "Scorer", $"Score {request.Value} for Game {id}");

        return CreatedAtAction(nameof(GetGame), new { id = score.Id }, score);
    }
}
