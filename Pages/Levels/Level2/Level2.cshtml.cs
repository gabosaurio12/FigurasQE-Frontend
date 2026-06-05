using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages;

public class Level2Model : PageModel
{
    public LevelAnswer Answer { get; private set; } = new LevelAnswer();
    public string NextLevelRoute { get; set; }
    public bool IsGuest { get; set; }
    public int Tries { get; set; }
    public bool Completed { get; set; } = false;
    public int SessionId { get; set; }
    public void OnGet()
    {
        Answer.Left = 2;
        Answer.Right = 1;
        Answer.Total = Answer.Left + Answer.Right;
        NextLevelRoute = "/Levels/Level3/Level3";
        IsGuest = !(User.Identity?.IsAuthenticated ?? false);
        if (!IsGuest)
        {
            SessionId = HttpContext.Session.GetInt32("sessionId") ?? 0;
        }
    }
}

