using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authentication;

namespace FigurasQE_WebClient.Pages.User;

public class LoginModel : PageModel
{
    private HttpClient Client;
    private string LoginRoute = "http://localhost:3000/auth/login";

    [BindProperty]
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Email { get; set; }


    [BindProperty]
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; }

    public LoginModel(HttpClient client)
    {
        Client = client;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await Client.PostAsJsonAsync(
            LoginRoute,
            new { Email, Password }
        );

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Credenciales Inválidas");

            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ModelState.AddModelError(string.Empty, "Error procesando la respuesta del servidor");
            return Page();
        }

        await SaveTokenInCookie(result.Token);
        var role = await GetRole(response);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        if (Equals(role, "student"))
            return RedirectToPage("/Student/Home");
        return RedirectToPage("/Tutor/Home");
    }

    private async Task SaveTokenInCookie(string token)
    {
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });
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
