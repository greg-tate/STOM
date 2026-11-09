using STOM.API.Models;

namespace STOM.API.Services;

public class BracketGenerator
{
    public Task<List<Tournament>> GenerateAsync(int competitionId)
    {
        return Task.FromResult(new List<Tournament>
        {
            new() { Id = competitionId, Name = "Generated Bracket", Status = "Active" }
        });
    }
}
