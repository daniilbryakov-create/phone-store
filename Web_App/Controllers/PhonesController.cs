using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Controllers;

// Просмотр и добавление — администратор и менеджер (альтернативный поток: модели нет в каталоге),
// изменение и удаление — только администратор.
[Authorize(Roles = "Admin,Manager")]
public class PhonesController : Controller
{
    private readonly ShopDbContext _context;

    public PhonesController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Phones.OrderBy(p => p.CompanyName).ThenBy(p => p.PhoneName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var phone = await _context.Phones.FirstOrDefaultAsync(m => m.Id == id);
        if (phone == null) return NotFound();

        return View(phone);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PhoneName,CompanyName,Price,Specification,StockAmount")] Phone phone)
    {
        await CheckUniqueAsync(phone);
        if (ModelState.IsValid)
        {
            _context.Add(phone);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(phone);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var phone = await _context.Phones.FindAsync(id);
        if (phone == null) return NotFound();

        return View(phone);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PhoneName,CompanyName,Price,Specification,StockAmount")] Phone phone)
    {
        if (id != phone.Id) return NotFound();

        await CheckUniqueAsync(phone);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(phone);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhoneExists(phone.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(phone);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var phone = await _context.Phones.FirstOrDefaultAsync(m => m.Id == id);
        if (phone == null) return NotFound();

        return View(phone);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var phone = await _context.Phones.FindAsync(id);
        if (phone == null) return RedirectToAction(nameof(Index));

        try
        {
            _context.Phones.Remove(phone);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // на телефон есть ссылки из заказов, продаж или поставок (DeleteBehavior.Restrict)
            ModelState.AddModelError("", "Нельзя удалить телефон: он используется в заказах, продажах или поставках.");
            return View("Delete", phone);
        }
        return RedirectToAction(nameof(Index));
    }

    private bool PhoneExists(int id) => _context.Phones.Any(e => e.Id == id);

    /// <summary>Модель (производитель + название) не должна повторяться в каталоге.</summary>
    private async Task CheckUniqueAsync(Phone phone)
    {
        var duplicate = await _context.Phones.AnyAsync(p =>
            p.Id != phone.Id && p.CompanyName == phone.CompanyName && p.PhoneName == phone.PhoneName);
        if (duplicate)
            ModelState.AddModelError("", $"Модель «{phone.CompanyName} {phone.PhoneName}» уже есть в каталоге.");
    }
}
