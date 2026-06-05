using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages;

public class Level3Model : PageModel
{
    public LevelAnswer Answer { get; private set; } = new LevelAnswer();
    public string NextLevelRoute { get; set; }
    public bool IsGuest { get; set; }
    public int Tries { get; set; }
    public bool Completed { get; set; } = false;
    public int SessionId { get; set; }

    public void OnGet()
    {
        Answer.Left = 1;
        Answer.Right = 2;
        Answer.Total = Answer.Left + Answer.Right;
        NextLevelRoute = "/Levels/Level4/Level4";
        IsGuest = !(User.Identity?.IsAuthenticated ?? false);
        if (!IsGuest)
        {
            SessionId = HttpContext.Session.GetInt32("sessionId") ?? 0;
        }
    }
}

