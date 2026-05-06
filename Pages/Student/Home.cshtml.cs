using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages.Student;

[Authorize(Roles = "student")]
public class HomeModel : PageModel
{
    private HttpClient Client;
    private string StudentRoute = "http://localhost:3000/data/students/";

    [BindProperty]
    public string StudentName { get; set; }

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

        var student = await Client.GetFromJsonAsync<StudentDto>(
            StudentRoute + userId
        );

        HttpContext.Session.SetString("student", JsonSerializer.Serialize(student));

        StudentName = student == null ? "Estudiante" : student.Name;

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

