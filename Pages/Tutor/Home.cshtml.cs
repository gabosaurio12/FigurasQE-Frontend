using System.Net.Http.Headers;
using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages.Tutor;

[AllowAnonymous]
public class HomeModel : PageModel
{
    private HttpClient Client;
    private string TutorRoute = "http://localhost:3000/data/tutors/";

    [BindProperty]
    public string TutorName { get; set; }

    public HomeModel(HttpClient http)
    {
        Client = http;
    }

    public async Task<IActionResult> OnGet()
    {
        var token = User.FindFirst("jwt_token")?.Value;
        var userId = User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            return RedirectToPage("/User/Login");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{TutorRoute}{userId}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return RedirectToPage("/User/Login");

        var tutor = await response.Content.ReadFromJsonAsync<TutorDto>();

        HttpContext.Session.SetString("tutor", JsonSerializer.Serialize(tutor));

        TutorName = tutor?.Name ?? "Tutor";

        return Page();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        context.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        context.HttpContext.Response.Headers["Expires"] = "0";
    }
}

