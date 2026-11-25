using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models.ViewModels;
using Web_App.Services;

namespace Web_App.Controllers;

// Поставщиков менеджер ведёт в SuppliersController, новые модели добавляет в PhonesController (Create).
[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    private readonly ShopDbContext _db;
    private readonly SupplyService _supplies;
    private readonly ReportService _reports;

    public ManagerController(ShopDbContext db, SupplyService supplies, ReportService reports)
    {
        _db = db;
        _supplies = supplies;
        _reports = reports;
    }

    public IActionResult Index() => View();

    // ---- остатки (view_stock) ----

    public async Task<IActionResult> Stock() =>
        View(await _db.Phones.OrderBy(p => p.CompanyName).ThenBy(p => p.PhoneName).ToListAsync());

    // ---- оформление поставки (order_phones) ----

    [HttpGet]
    public async Task<IActionResult> NewSupply() => View(await BuildSupplyFormAsync(null));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NewSupply(SupplyCreateViewModel model)
    {
        try
        {
            var supply = await _supplies.CreateAsync(User.GetUserId(), model.SupplierId,
                model.Lines.Select(l => new SupplyLineInput(l.PhoneId, l.Amount, l.PurchaseCost)),
                model.ReceiveNow);
            TempData["Success"] = model.ReceiveNow
                ? $"Поставка № {supply.Id} принята, сумма {supply.TotalPurchaseCost:N0} руб., остатки обновлены."
                : $"Поставка № {supply.Id} оформлена, сумма {supply.TotalPurchaseCost:N0} руб.";
            return RedirectToAction(nameof(Supplies));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }
        return View(await BuildSupplyFormAsync(model));
    }

    // ---- управление поставками (manage_supply) ----

    public async Task<IActionResult> Supplies() => View(await _supplies.ListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SupplyAction(int id, string action)
    {
        try
        {
            switch (action)
            {
                case "receive":
                    await _supplies.ReceiveAsync(id);
                    TempData["Success"] = $"Поставка № {id} принята на склад.";
                    break;
                case "cancel":
                    await _supplies.CancelAsync(id);
                    TempData["Success"] = $"Поставка № {id} отменена.";
                    break;
                default:
                    TempData["Error"] = "Неизвестное действие.";
                    break;
            }
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Supplies));
    }

    // ---- отчёты (create_report) ----

    public async Task<IActionResult> Reports() => View(await _reports.ListAsync());

    [HttpGet]
    public IActionResult NewReport() => View(new ReportCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NewReport(ReportCreateViewModel model)
    {
        try
        {
            var report = await _reports.CreateAsync(User.GetUserId(), model.DateFrom, model.DateTo, model.Type);
            return RedirectToAction(nameof(ReportDetails), new { id = report.Id });
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> ReportDetails(int id)
    {
        var report = await _reports.GetAsync(id);
        if (report == null) return NotFound();
        return View(report);
    }

    // ---- вспомогательное ----

    private async Task<SupplyCreateViewModel> BuildSupplyFormAsync(SupplyCreateViewModel? posted)
    {
        var postedLines = (posted?.Lines ?? new List<SupplyLine>())
            .GroupBy(l => l.PhoneId)
            .ToDictionary(g => g.Key, g => g.First());

        var phones = await _db.Phones.OrderBy(p => p.CompanyName).ThenBy(p => p.PhoneName).ToListAsync();
        var model = posted ?? new SupplyCreateViewModel();
        model.Suppliers = await _db.Suppliers.OrderBy(s => s.OrganizationName).ToListAsync();
        model.Lines = phones.Select(p =>
        {
            postedLines.TryGetValue(p.Id, out var old);
            return new SupplyLine
            {
                PhoneId = p.Id,
                Title = p.ToString(),
                Price = p.Price,
                Stock = p.StockAmount,
                Amount = old?.Amount ?? 0,
                PurchaseCost = old?.PurchaseCost ?? 0
            };
        }).ToList();
        return model;
    }
}
