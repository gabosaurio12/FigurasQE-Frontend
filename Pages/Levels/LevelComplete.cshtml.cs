using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages;

public class LevelCompleteModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string NextLevel { get; set; }

    public void OnGet()
    {

    }
}