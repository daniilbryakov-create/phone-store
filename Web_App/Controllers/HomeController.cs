using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models.ViewModels;

namespace Web_App.Controllers;

// Если в вашем HomeController были Privacy/Error — оставьте их, замените только Index и добавьте Details.
public class HomeController : Controller
{
    private readonly ShopDbContext _db;

    public HomeController(ShopDbContext db) => _db = db;

    /// <summary>Поиск телефонов (доступен без входа): все критерии необязательны и объединяются по «И».</summary>
    public async Task<IActionResult> Index(PhoneSearchViewModel model)
    {
        if (model.MinPrice.HasValue && model.MaxPrice.HasValue && model.MinPrice > model.MaxPrice)
        {
            ModelState.AddModelError("", "Минимальная цена не может быть больше максимальной.");
            return View(model);
        }

        var query = _db.Phones.AsQueryable();

        var company = model.Company?.Trim();
        var name = model.ModelName?.Trim();
        if (!string.IsNullOrEmpty(company)) query = query.Where(p => p.CompanyName.Contains(company));
        if (!string.IsNullOrEmpty(name)) query = query.Where(p => p.PhoneName.Contains(name));
        if (model.MinPrice.HasValue) query = query.Where(p => p.Price >= model.MinPrice.Value);
        if (model.MaxPrice.HasValue) query = query.Where(p => p.Price <= model.MaxPrice.Value);
        if (model.OnlyAvailable) query = query.Where(p => p.StockAmount > 0);

        model.Results = await query.OrderBy(p => p.Price).ThenBy(p => p.PhoneName).ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var phone = await _db.Phones.FindAsync(id);
        if (phone == null) return NotFound();
        return View(phone);
    }
}
