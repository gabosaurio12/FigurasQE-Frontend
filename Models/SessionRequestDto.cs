namespace FigurasQE_WebClient.Models;

public class SessionRequestDto
{
    public int IdStudent { get; set; }

    public DateTime? BeginningDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Device { get; set; }

    public int LevelsPlayed { get; set; }

    public int LevelsCompleted { get; set; }

    public int DurationMinutes { get; set; }
}