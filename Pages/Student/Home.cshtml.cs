using System.Diagnostics;
using System.Text.Json;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages.Student;

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

    public async Task OnGet()
    {
        var token = Request.Cookies["jwt"];

        if (string.IsNullOrEmpty(token))
        {
            Response.Redirect("/User/Login");
            return;
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
    }
}

