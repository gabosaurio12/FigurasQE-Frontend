using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FigurasQE_WebClient.Pages;

public class SignupModel : PageModel
{
    [BindProperty]
    public int AgeValue { get; set; }
    [BindProperty]
    public string Country { get; set; }

    public List<SelectListItem> Countries { get; set; }

    public void OnGet()
    {
        AgeValue = 5;
        Countries = new List<SelectListItem>
        {
            new("México", "MX"),
            new("Estados Unidos", "US"),
            new("España", "ES")
        };
    }
}

