using System.Net.Http.Headers;
using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages.Tutor;

[Authorize(Roles = "tutor")]
public class SessionsDetailsModel : PageModel
{
    private readonly HttpClient Client;

    private string StudentRoute = "http://localhost:3000/data/students/";
    private string SessionsRoute = "http://localhost:3000/data/students/";

    public StudentDto? Student { get; set; }
    public List<SessionDto> Sessions { get; set; } = new();

    public SessionsDetailsModel(HttpClient http)
    {
        Client = http;
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var token = User.FindFirst("jwt_token")?.Value;

        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/User/Login");

        var studentRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{StudentRoute}{id}"
        );

        studentRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var studentResponse = await Client.SendAsync(studentRequest);

        if (!studentResponse.IsSuccessStatusCode)
            return RedirectToPage("/Tutor/Students");

        var studentJson = await studentResponse.Content.ReadAsStringAsync();

        Student = JsonSerializer.Deserialize<StudentDto>(
            studentJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        var sessionRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{SessionsRoute}{id}/sessions"
        );

        sessionRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var sessionResponse = await Client.SendAsync(sessionRequest);

        if (sessionResponse.IsSuccessStatusCode)
        {
            var sessionJson = await sessionResponse.Content.ReadAsStringAsync();

            Sessions = JsonSerializer.Deserialize<List<SessionDto>>(
                sessionJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();
        }

        return Page();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        context.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        context.HttpContext.Response.Headers["Expires"] = "0";
    }
}