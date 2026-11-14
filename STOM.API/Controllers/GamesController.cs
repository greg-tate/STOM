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
    private readonly ILogger<GamesController> _logger;

    public GamesController(StomDbContext context, AuditService auditService, ILogger<GamesController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetAllGames()
    {
        try
        {
            var games = await _context.Games.Include(g => g.Scores).ToListAsync();
            _logger.LogInformation("Retrieved {GameCount} games", games.Count);
            await _auditService.LogAsync("GET_ALL_GAMES", null, "System", $"Retrieved {games.Count} games");
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all games");
            await _auditService.LogAsync("GET_ALL_GAMES_ERROR", null, "System", $"Error retrieving games: {ex.Message}");
            throw;
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(int id)
    {
        try
        {
            var game = await _context.Games
                .Include(g => g.Scores)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                _logger.LogWarning("Game {GameId} not found", id);
                await _auditService.LogAsync("GET_GAME_NOT_FOUND", id, "System", $"Game {id} not found");
                return NotFound();
            }

            _logger.LogInformation("Retrieved game {GameId}", id);
            await _auditService.LogAsync("GET_GAME", id, "System", $"Retrieved game {id}");
            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game {GameId}", id);
            await _auditService.LogAsync("GET_GAME_ERROR", id, "System", $"Error retrieving game {id}: {ex.Message}");
            throw;
        }
    }

    [HttpPost("{id}/scores")]
    public async Task<ActionResult<Score>> AddScore(int id, AddScoreRequest request)
    {
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                _logger.LogWarning("Cannot add score: Game {GameId} not found", id);
                await _auditService.LogAsync("ADD_SCORE_GAME_NOT_FOUND", id, "Scorer", $"Game {id} not found");
                return NotFound("Game not found");
            }

            var score = new Score { GameId = id, Value = request.Value, CreatedAt = DateTime.UtcNow };
            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added score {ScoreId} with value {ScoreValue} to game {GameId}", score.Id, request.Value, id);
            await _auditService.LogAsync("SCORE_ADDED", score.Id, "Scorer", $"Score {request.Value} added to game {id}");

            return CreatedAtAction(nameof(GetGame), new { id = score.Id }, score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding score to game {GameId}", id);
            await _auditService.LogAsync("ADD_SCORE_ERROR", id, "Scorer", $"Error adding score to game {id}: {ex.Message}");
            throw;
        }
    }
}
