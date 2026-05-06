using System.ComponentModel.DataAnnotations;
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

    [BindProperty]
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
    ErrorMessage = "Debe tener mínimo 8 caracteres, mayúscula, minúscula, número y símbolo")]
    public string Password { get; set; }

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
        Input.Password = Password;

        Console.WriteLine(Input.Password);

        if (!ModelState.IsValid)
            return Page();

        using var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "http://localhost:3000/auth/register",
            Input
        );

        Console.WriteLine(response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();

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

