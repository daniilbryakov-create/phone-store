using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Services;

/// <summary>Администратор: создание, изменение и удаление учётных записей.</summary>
public class UserService
{
    private readonly ShopDbContext _db;
    public UserService(ShopDbContext db) => _db = db;

    public Task<List<User>> ListAsync() => _db.Users.OrderBy(u => u.Id).ToListAsync();

    public Task<User?> GetAsync(int id) => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User> CreateAsync(UserRole role, string? fullName, string? login, string? password,
        string? phoneNumber, string? email)
    {
        var name = Rules.NotBlank(fullName, "ФИО");
        var l = Rules.Login(login);
        var pw = Rules.Password(password);
        await EnsureLoginFreeAsync(l, 0);

        User user;
        switch (role)
        {
            case UserRole.Seller: user = new Seller(); break;
            case UserRole.Manager: user = new Manager(); break;
            case UserRole.Admin: user = new Admin(); break;
            case UserRole.Client:
                var number = Rules.PhoneNumber(phoneNumber);
                if (await _db.Clients.AnyAsync(c => c.PhoneNumber == number))
                    throw new BusinessException($"Клиент с номером {number} уже зарегистрирован.");
                user = new Client { PhoneNumber = number, Email = Rules.OptionalEmail(email) };
                break;
            default:
                throw new BusinessException("Неизвестная роль пользователя.");
        }

        user.FullName = name;
        user.Login = l;
        user.PasswordHash = PasswordHasher.Hash(pw);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    /// <summary>change_user(): пустые поля не меняются.</summary>
    public async Task UpdateAsync(int id, string? fullName, string? login, string? password)
    {
        var user = await GetAsync(id) ?? throw new BusinessException("Пользователь не найден.");

        var newName = string.IsNullOrWhiteSpace(fullName) ? null : Rules.NotBlank(fullName, "ФИО");
        var newLogin = string.IsNullOrWhiteSpace(login) ? null : Rules.Login(login);
        var newPassword = string.IsNullOrEmpty(password) ? null : Rules.Password(password);

        if (newLogin is not null)
            await EnsureLoginFreeAsync(newLogin, user.Id);
        if (!user.HasAccount && (newLogin is null) != (newPassword is null))
            throw new BusinessException("Чтобы создать учётную запись, укажите и логин, и пароль.");

        if (newName is not null) user.FullName = newName;
        if (newLogin is not null) user.Login = newLogin;
        if (newPassword is not null) user.PasswordHash = PasswordHasher.Hash(newPassword);

        await _db.SaveChangesAsync();
    }

    /// <summary>delete_user(): нельзя удалить себя и пользователя, который есть в документах.</summary>
    public async Task DeleteAsync(int id, int currentUserId)
    {
        var user = await GetAsync(id) ?? throw new BusinessException("Пользователь не найден.");
        if (user.Id == currentUserId)
            throw new BusinessException("Нельзя удалить собственную учётную запись.");

        var used = false;
        if (user is Client)
            used = await _db.Orders.AnyAsync(o => o.ClientId == id) || await _db.Sales.AnyAsync(s => s.ClientId == id);
        else if (user is Seller)
            used = await _db.Sales.AnyAsync(s => s.SellerId == id) || await _db.Returns.AnyAsync(r => r.SellerId == id);
        else if (user is Manager)
            used = await _db.Supplies.AnyAsync(s => s.ManagerId == id) || await _db.Reports.AnyAsync(r => r.ManagerId == id);

        if (used)
            throw new BusinessException(
                $"Пользователя «{user.FullName}» нельзя удалить: он указан в заказах, продажах, поставках или отчётах.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureLoginFreeAsync(string login, int exceptId)
    {
        if (await _db.Users.AnyAsync(u => u.Login == login && u.Id != exceptId))
            throw new BusinessException($"Логин «{login}» уже занят.");
    }
}
