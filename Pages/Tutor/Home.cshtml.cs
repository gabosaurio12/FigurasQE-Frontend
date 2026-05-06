using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace FigurasQE_WebClient.Pages.Tutor;

[Authorize(Roles = "tutor")]
public class HomeModel : PageModel
{
    private HttpClient Client;
    private string TutorRoute = "http://localhost:3000/data/students/";

    [BindProperty]
    public string TutorName { get; set; }

    public HomeModel(HttpClient http)
    {
        Client = http;
    }

    public async Task<IActionResult> OnGet()
    {
        var token = Request.Cookies["jwt"];

        if (string.IsNullOrEmpty(token))
        {
            Response.Redirect("/User/Login");
            return Page();
        }

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var userId = jwt.Claims.First(c => c.Type == "sub").Value;

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var tutor = await Client.GetFromJsonAsync<StudentDto>(
            TutorRoute + userId
        );

        HttpContext.Session.SetString("tutor", JsonSerializer.Serialize(tutor));

        TutorName = tutor == null ? "Tutor" : tutor.Name;

        return Page();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!context.HttpContext.Request.Cookies.ContainsKey("jwt"))
        {
            context.Result = new RedirectToPageResult("/User/Login");
            return;
        }

        context.HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        context.HttpContext.Response.Headers["Expires"] = "0";
    }
}

