using System.Text.Json.Serialization;

namespace STOM.API.Models;

public class Score
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public Game? Game { get; set; }
}
