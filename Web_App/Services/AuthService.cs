using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Services;

/// <summary>Прецедент «Авторизоваться»: поиск учётной записи по логину и проверка пароля.</summary>
public class AuthService
{
    private readonly ShopDbContext _db;
    public AuthService(ShopDbContext db) => _db = db;

    /// <summary>Возвращает пользователя или null, если логин/пароль неверны.</summary>
    public async Task<User?> LoginAsync(string? login, string? password)
    {
        if (string.IsNullOrWhiteSpace(login) || password is null) return null;

        var l = login.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == l);
        return user is not null && user.HasAccount && PasswordHasher.Verify(password, user.PasswordHash)
            ? user
            : null;
    }
}
