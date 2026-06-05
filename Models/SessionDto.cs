namespace FigurasQE_WebClient.Models;

public class SessionDto
{
    public int IdSession { get; set; }
    public int IdStudent { get; set; }

    public DateTime? BeginningDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Device { get; set; }

    public int LevelsPlayed { get; set; }

    public int LevelsCompleted { get; set; }

    public int DurationMinutes { get; set; }
}