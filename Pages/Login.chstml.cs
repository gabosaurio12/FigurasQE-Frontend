using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages;

public class LoginModel : PageModel
{

    [BindProperty]
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Email { get; set; }


    [BindProperty]
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; }

    public void OnGet()
    {

    }

    public async void OnPost()
    {
        var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "http://localhost:3000/auth/login",
            new { Email, Password }
        );

        Console.WriteLine(response); // se va
    }
}
