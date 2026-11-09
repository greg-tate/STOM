namespace STOM.API.Models;

public class AddScoreRequest
{
    public int Value { get; set; }
}

public class CompetitionRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class ImportResult
{
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
