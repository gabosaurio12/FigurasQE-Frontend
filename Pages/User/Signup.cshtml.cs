using System.Net.Http.Headers;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FigurasQE_WebClient.Pages.User;

public class SignupModel : PageModel
{
    [BindProperty]
    public SignupRequest Input { get; set; }

    public List<SelectListItem> Countries { get; set; }

    private List<SelectListItem> GetCountries() => new()
    {
        new("México", "MX"),
        new("Estados Unidos", "US"),
        new("España", "ES")
    };

    public void OnGet()
    {
        Input = new SignupRequest
        {
            AgeValue = 5,
            Role = "student",
        };

        Countries = GetCountries();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Countries = GetCountries();

        if (!ModelState.IsValid)
            return Page();

        var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "http://localhost:3000/auth/register",
            Input
        );

        Console.WriteLine(response);

        return RedirectToPage("/User/Login");
    }
}

