using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_App.Models;
using Web_App.Models.ViewModels;
using Web_App.Services;

namespace Web_App.Controllers;

// Каталог телефонов администратор ведёт в PhonesController (scaffold), здесь — пользователи.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserService _users;

    public AdminController(UserService users) => _users = users;

    public IActionResult Index() => View();

    public async Task<IActionResult> Users() => View(await _users.ListAsync());

    // ---- создание ----

    [HttpGet]
    public IActionResult CreateUser() => View(new UserCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var user = await _users.CreateAsync(model.Role, model.FullName, model.Login, model.Password,
                model.PhoneNumber, model.Email);
            TempData["Success"] = $"Пользователь «{user.FullName}» создан.";
            return RedirectToAction(nameof(Users));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Не удалось сохранить: логин или телефон уже используются.");
        }
        return View(model);
    }

    // ---- изменение ----

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _users.GetAsync(id);
        if (user == null) return NotFound();
        return View(new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Login = user.Login,
            RoleTitle = user.Kind.ToDisplayString()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserEditViewModel model)
    {
        var user = await _users.GetAsync(model.Id);
        if (user == null) return NotFound();
        model.RoleTitle = user.Kind.ToDisplayString();

        try
        {
            await _users.UpdateAsync(model.Id, model.FullName, model.Login, model.Password);
            TempData["Success"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Users));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Не удалось сохранить: такой логин уже используется.");
        }
        return View(model);
    }

    // ---- удаление ----

    [HttpGet]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _users.GetAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("DeleteUser"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id)
    {
        try
        {
            await _users.DeleteAsync(id, User.GetUserId());
            TempData["Success"] = "Пользователь удалён.";
            return RedirectToAction(nameof(Users));
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(DeleteUser), new { id });
        }
    }
}
