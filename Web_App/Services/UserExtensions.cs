using System.Security.Claims;

namespace Web_App.Services;

public static class UserExtensions
{
    /// <summary>Id текущего пользователя из cookie-аутентификации.</summary>
    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? throw new InvalidOperationException("Пользователь не авторизован."));
}
