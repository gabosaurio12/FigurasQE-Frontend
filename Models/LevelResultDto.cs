namespace FigurasQE_WebClient.Models;

public class LevelResultDto
{
    public int IdResult { get; set; }
    public int IdSession { get; set; }
    public int IdLevel { get; set; }
    public int? FinishingTime { get; set; }
    public int? Attempts { get; set; }
    public int? Fails { get; set; }
    public bool? Completed { get; set; }
    public SessionDto? Session { get; set; }
}