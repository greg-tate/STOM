namespace STOM.API.Models;

public class Competition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Planning"; // Planning, Active, Completed
    public List<Game> Games { get; set; } = new();
}
