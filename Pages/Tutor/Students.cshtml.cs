using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages.Tutor;

[AllowAnonymous]
public class StudentsModel : PageModel
{
    private readonly HttpClient Client;

    private string TutorRoute = "http://localhost:3000/data/tutors/";
    private string AssignStudentRoute = "http://localhost:3000/data/tutors/assign-student";

    public List<StudentDto> Students { get; set; } = new();

    [BindProperty]
    public string StudentEmail { get; set; }

    public string TutorName { get; set; } = "Tutor";

    public StudentsModel(HttpClient http)
    {
        Client = http;
    }

    public async Task<IActionResult> OnGet()
    {
        var token = User.FindFirst("jwt_token")?.Value;
        var userId = User.FindFirst("sub")?.Value;
        TutorName = User.FindFirst("name")?.Value ?? "Tutor";

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            return RedirectToPage("/User/Login");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TutorRoute}{userId}/students"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return RedirectToPage("/User/Login");

        var json = await response.Content.ReadAsStringAsync();

        Students = JsonSerializer.Deserialize<List<StudentDto>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new List<StudentDto>();

        return Page();
    }

    public async Task<IActionResult> OnPostAssignStudent()
    {
        var token = User.FindFirst("jwt_token")?.Value;
        var tutorEmail = User.Identity?.Name;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tutorEmail))
            return RedirectToPage("/User/Login");

        var body = new
        {
            studentEmail = StudentEmail,
            tutorEmail = tutorEmail
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            AssignStudentRoute
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var response = await Client.SendAsync(request);

        return RedirectToPage();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        context.HttpContext.Response.Headers["Cache-Control"] =
            "no-store, no-cache, must-revalidate";

        context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        context.HttpContext.Response.Headers["Expires"] = "0";
    }
}