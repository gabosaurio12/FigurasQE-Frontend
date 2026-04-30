using System.Net.Http.Headers;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FigurasQE_WebClient.Pages.User;

public class SignupModel : PageModel
{
    [BindProperty]
    public SignupRequest Input { get; set; }

    public List<SelectListItem> Countries { get; set; }

    private List<SelectListItem> GetCountries() => new()
    {
        new("México", "MX"),
        new("Estados Unidos", "US"),
        new("España", "ES")
    };

    public List<SelectListItem> Neurodivergencies { get; set; }
    private List<SelectListItem> GetNeurodivergencies() => new()
    {
        new("Autismo", "autismo"),
        new("TDA", "tda"),
        new("TDAH", "tdah"),
        new("Hiperactividad", "hiperactividad"),
        new("Ninguna", "ninguna"),
        new("Otra", "otra")
    };

    public void OnGet()
    {
        Input = new SignupRequest
        {
            Age = 5,
            Role = "student",
        };

        Countries = GetCountries();
        Neurodivergencies = GetNeurodivergencies();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Countries = GetCountries();
        Neurodivergencies = GetNeurodivergencies();

        if (!ModelState.IsValid)
            return Page();

        using var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "http://localhost:3000/auth/register",
            Input
        );

        // 👇 SI FALLA EL BACKEND
        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();

            // intenta leer message del JSON
            var errorMessage = "Error al registrar usuario";

            try
            {
                var parsed = System.Text.Json.JsonDocument.Parse(errorJson);
                if (parsed.RootElement.TryGetProperty("message", out var msg))
                {
                    errorMessage = msg.GetString() ?? errorMessage;
                }
            }
            catch { }

            ModelState.AddModelError(string.Empty, errorMessage);
            return Page();
        }

        return RedirectToPage("/User/Login");
    }
}

