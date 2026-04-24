namespace FigurasQE_WebClient.Models;

public class SignupRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Genre { get; set; }
    public string Country { get; set; }
    public int AgeValue { get; set; }
    public string Role { get; set; }
    public string? Neurodivergency { get; set; }
    public string? Grade { get; set; }
}