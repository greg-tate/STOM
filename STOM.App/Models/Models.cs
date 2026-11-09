namespace STOM.App.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Score> Scores { get; set; } = new();
}

public class Score
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Game? Game { get; set; }
}

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Planning";
    public List<Game> Games { get; set; } = new();
}

public class Competition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Planning"; // Planning, Active, Completed
    public List<Game> Games { get; set; } = new();
}

public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

public class AddScoreRequest
{
    public int Value { get; set; }
}

public class CompetitionRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
