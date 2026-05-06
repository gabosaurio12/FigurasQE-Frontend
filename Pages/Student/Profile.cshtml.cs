using System.Diagnostics;
using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FigurasQE_WebClient.Pages.Student;

[Authorize(Roles = "student")]
public class ProfileModel : PageModel
{
    private HttpClient Client;
    private string StudentRoute = "http://localhost:3000/data/students/";

    [BindProperty]
    public StudentDto Student { get; set; } = new();
    public List<SelectListItem> Genres { get; set; }
    private List<SelectListItem> GetGenres() => new()
    {
        new("Másculino", "M"),
        new("Femenino Unidos", "F"),
        new("Otro", "O")
    };
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

    public string? ErrorMessage { get; set; }


    public ProfileModel(HttpClient http)
    {
        Client = http;
    }

    public async Task<IActionResult> OnGet()
    {
        Neurodivergencies = GetNeurodivergencies();
        Countries = GetCountries();
        Genres = GetGenres();

        var token = Request.Cookies["jwt"];

        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = "No hay sesión activa";
            return Page();
        }

        var json = HttpContext.Session.GetString("student");
        if (string.IsNullOrEmpty(json))
        {
            ErrorMessage = "No se pudo cargar tu perfil";
            return Page();
        }

        var student = JsonSerializer.Deserialize<StudentDto>(json);

        if (student == null)
        {
            ErrorMessage = "Hubo un error al cargar tu perfil, intenta más tarde por favor";
            return Page();
        }

        Student = student;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var token = Request.Cookies["jwt"];

        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = "No autorizado";
            return Page();
        }

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{StudentRoute}{Student.IdStudent}"
        );

        request.Content = new StringContent(
            JsonSerializer.Serialize(Student),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Error al actualizar el perfil";
            return Page();
        }

        var updated = await response.Content.ReadFromJsonAsync<StudentDto>();
        if (updated != null)
            Student = updated;

        ErrorMessage = "Perfil actualizado correctamente ✅";
        return Page();
    }
}

