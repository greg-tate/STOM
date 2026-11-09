namespace STOM.API.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Scorer"; // Scorer, Admin, SuperAdmin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
