using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using FigurasQE_WebClient.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FigurasQE_WebClient.Pages;

public class LevelsCatalogModel : PageModel
{
    private HttpClient Client;
    private string SessionsRoute = "http://localhost:3000/data/sessions/";
    private string StudentsRoute = "http://localhost:3000/data/students/";
    public bool IsGuest { get; set; }
    public DateTime BeginningDate { get; set; }
    public string? SuccessMessage { get; set; }
    public string? WarningMessage { get; set; }
    private readonly JsonSerializerOptions JsonSerializerOpts = new() { PropertyNameCaseInsensitive = true };

    public LevelsCatalogModel(HttpClient http)
    {
        Client = http;
    }

    public async Task OnGetAsync()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
            return;

        var token = User.FindFirst("jwt_token")?.Value;
        var id = User.FindFirst("sub")?.Value;

        if (!int.TryParse(id, out int intId))
            return;

        var current = await GetOpenSession(intId);

        if (current == null)
        {
            var response = await CreateSession(token, intId);

            if (!response.IsSuccessStatusCode)
            {
                WarningMessage = "No se pudo crear la sesión.";
                return;
            }

            current = await GetLatestSession(intId);
        }

        HttpContext.Session.SetInt32("sessionId", current.IdSession);
    }

    private async Task<SessionDto?> GetOpenSession(int studentId)
    {
        var sessions = await GetSessions();

        return sessions.FirstOrDefault(s => s.EndDate == null);
    }

    private async Task<SessionDto> GetLatestSession(int studentId)
    {
        var sessions = await GetSessions();

        return sessions
            .OrderByDescending(s => s.BeginningDate)
            .First();
    }

    public async Task OnPostEndSessionAsync()
    {
        var session = await GetCurrentSession();

        if (session == null)
        {
            WarningMessage = "Hubo un error al actualizar la sesión, contacte a soporte técnico";
            return;
        }

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{SessionsRoute}{session.IdSession}"
        );

        var token = User.FindFirst("jwt_token")?.Value;

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        session.EndDate = DateTime.Now;

        request.Content = new StringContent(
            JsonSerializer.Serialize(session),
            Encoding.UTF8,
            "application/json"
        );

        var response = await Client.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Body: {body}");

        if (!response.IsSuccessStatusCode)
        {
            WarningMessage = "Hubo un error al terminar la sesión. Los resultados de esta sesión no serán registrados.";
            return;
        }

        HttpContext.Session.Remove("sessionId");

        SuccessMessage = "¡Se terminó la sesión con éxito!";
    }

    private async Task<HttpResponseMessage> CreateSession(string token, int id)
    {
        var session = new SessionRequestDto
        {
            IdStudent = id,
            BeginningDate = DateTime.Now,
            Device = GetDevice()
        };

        var request = new HttpRequestMessage(HttpMethod.Post, SessionsRoute);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = new StringContent(
            JsonSerializer.Serialize(session),
            Encoding.UTF8,
            "application/json"
        );

        return await Client.SendAsync(request);
    }

    private async Task<SessionDto?> GetCurrentSession()
    {
        var id = HttpContext.Session.GetInt32("sessionId");
        if (!id.HasValue)
        {
            WarningMessage = "No hay sesión activa";
            return null;
        }

        var token = User.FindFirst("jwt_token")?.Value;
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{SessionsRoute}{id}"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            WarningMessage = "Hubo un error al actualizar la sesión, contacte a soporte técnico";
            return null;
        }

        var sessionJson = await response.Content.ReadAsByteArrayAsync();
        var session = JsonSerializer.Deserialize<SessionDto>(
            sessionJson,
            JsonSerializerOpts
        ) ?? new SessionDto();
        return session;
    }

    private async Task<List<SessionDto>> GetSessions()
    {
        var id = User.FindFirst("sub")?.Value;
        var sessionRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{StudentsRoute}{id}/sessions"
        );

        var token = User.FindFirst("jwt_token")?.Value;
        sessionRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var sessionResponse = await Client.SendAsync(sessionRequest);

        if (sessionResponse.IsSuccessStatusCode)
        {
            var sessionJson = await sessionResponse.Content.ReadAsStringAsync();

            var sessions = JsonSerializer.Deserialize<List<SessionDto>>(
                sessionJson,
                JsonSerializerOpts
            ) ?? [];

            return sessions;
        }
        return [];
    }

    private string GetDevice()
    {
        var userAgent = Request.Headers.UserAgent.ToString();

        if (userAgent.Contains("Android") &&
            !userAgent.Contains("Mobile"))
        {
            return "Tablet";
        }
        else if (userAgent.Contains("iPad"))
        {
            return "Tablet";
        }
        else if (userAgent.Contains("Mobile"))
        {
            return "Phone";
        }
        else
        {
            return "Desktop";
        }
    }
}

