using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FigurasQE_WebClient.Pages.User;

public class LoginModel : PageModel
{

    [BindProperty]
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Email { get; set; }


    [BindProperty]
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPost()
    {
        var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "http://localhost:3000/auth/login",
            new { Email, Password }
        );

        Console.WriteLine(response);

        var role = await GetRole(response);
        if (Equals(role, "student"))
            return RedirectToPage("/Student/MainPage");
        return RedirectToPage("/Tutor/MainPage");
    }

    private async Task<string> GetRole(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var json = await response.Content.ReadAsStringAsync();

        var token = System.Text.Json.JsonDocument.Parse(json)
            .RootElement
            .GetProperty("token")
            .GetString();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                   ?? jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        return role;
    }
}
