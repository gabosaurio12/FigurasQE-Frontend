using System.Net.Http.Headers;
using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages.Tutor;

[Authorize(Roles = "tutor")]
public class StudentDetailsModel : PageModel
{
    private readonly HttpClient Client;

    private string StudentRoute = "http://localhost:3000/data/students/";

    public StudentDto? Student { get; set; }

    public StudentDetailsModel(HttpClient http)
    {
        Client = http;
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var token = User.FindFirst("jwt_token")?.Value;

        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/User/Login");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{StudentRoute}{id}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return RedirectToPage("/Tutor/Students");

        var json = await response.Content.ReadAsStringAsync();

        Student = JsonSerializer.Deserialize<StudentDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return Page();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        context.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        context.HttpContext.Response.Headers["Expires"] = "0";
    }
}