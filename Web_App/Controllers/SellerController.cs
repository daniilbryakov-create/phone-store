using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_App.Data;
using Web_App.Models.ViewModels;
using Web_App.Services;

namespace Web_App.Controllers;

[Authorize(Roles = "Seller")]
public class SellerController : Controller
{
    private readonly ShopDbContext _db;
    private readonly SaleService _sales;
    private readonly OrderService _orders;

    public SellerController(ShopDbContext db, SaleService sales, OrderService orders)
    {
        _db = db;
        _sales = sales;
        _orders = orders;
    }

    public IActionResult Index() => View();

    // ---- продажа ----

    [HttpGet]
    public async Task<IActionResult> Sell() =>
        View(new SaleCreateViewModel { Lines = await LineResolver.BuildFormAsync(_db) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sell(SaleCreateViewModel model)
    {
        try
        {
            var sale = await _sales.SellAsync(User.GetUserId(),
                model.Lines.Select(l => new LineInput(l.PhoneId, l.Amount)),
                model.ClientPhone, model.ClientName, model.ClientEmail);
            return RedirectToAction(nameof(Receipt), new { id = sale.Id });
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        model.Lines = await LineResolver.BuildFormAsync(_db, model.Lines);
        return View(model);
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var sale = await _sales.GetAsync(id);
        if (sale == null) return NotFound();
        return View(sale);
    }

    // ---- регистрация клиента ----

    [HttpGet]
    public IActionResult RegisterClient() => View(new RegisterClientViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterClient(RegisterClientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var client = await _sales.RegisterClientAsync(model.FullName, model.PhoneNumber, model.Email);
            TempData["Success"] = $"Клиент зарегистрирован: {client.FullName}, {client.PhoneNumber}.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    // ---- история продаж и возвраты ----

    public async Task<IActionResult> Sales() => View(await _sales.HistoryAsync());

    [HttpGet]
    public async Task<IActionResult> ReturnItem(int saleItemId)
    {
        var item = await _sales.GetItemAsync(saleItemId);
        if (item == null) return NotFound();
        return View(ToReturnModel(item, new ReturnViewModel { SaleItemId = saleItemId }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnItem(ReturnViewModel model)
    {
        var item = await _sales.GetItemAsync(model.SaleItemId);
        if (item == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var ret = await _sales.ReturnAsync(User.GetUserId(), model.SaleItemId, model.Amount, model.Reason);
                TempData["Success"] = $"Возврат оформлен, к выплате {ret.RefundSum:N0} руб.";
                return RedirectToAction(nameof(Sales));
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(ToReturnModel(item, model));
    }

    private static ReturnViewModel ToReturnModel(Models.SaleItem item, ReturnViewModel model)
    {
        model.SaleId = item.SaleId;
        model.PhoneTitle = item.Phone.ToString();
        model.ReturnableAmount = item.ReturnableAmount;
        model.PricePerItem = item.PricePerItem;
        return model;
    }

    // ---- заказы клиентов ----

    public async Task<IActionResult> Orders() =>
        View(new OrdersListViewModel { Orders = await _orders.GetAllAsync(), SellerMode = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderAction(int id, string action)
    {
        try
        {
            switch (action)
            {
                case "confirm":
                    var ok = await _orders.ConfirmAsync(id);
                    TempData[ok ? "Success" : "Error"] = ok
                        ? $"Заказ № {id} подтверждён и ожидает оплаты."
                        : $"Заказ № {id}: товара нет в наличии, заказ отменён.";
                    break;
                case "pay":
                    await _orders.ConfirmPaymentAsync(id);
                    TempData["Success"] = $"Заказ № {id}: оплата получена.";
                    break;
                case "ready":
                    await _orders.MarkReadyForPickupAsync(id);
                    TempData["Success"] = $"Заказ № {id} готов к выдаче.";
                    break;
                case "complete":
                    await _orders.CompleteAsync(id);
                    TempData["Success"] = $"Заказ № {id} выполнен.";
                    break;
                case "cancel":
                    await _orders.CancelAsync(id);
                    TempData["Success"] = $"Заказ № {id} отменён.";
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
        return RedirectToAction(nameof(Orders));
    }
}
