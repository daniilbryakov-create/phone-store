using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_App.Data;
using Web_App.Models.ViewModels;
using Web_App.Services;

namespace Web_App.Controllers;

[Authorize(Roles = "Client")]
public class ClientController : Controller
{
    private readonly ShopDbContext _db;
    private readonly OrderService _orders;

    public ClientController(ShopDbContext db, OrderService orders)
    {
        _db = db;
        _orders = orders;
    }

    public IActionResult Index() => View();

    // ---- оформление заказа ----

    [HttpGet]
    public async Task<IActionResult> NewOrder() =>
        View(new OrderCreateViewModel { Lines = await LineResolver.BuildFormAsync(_db) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NewOrder(OrderCreateViewModel model)
    {
        try
        {
            var order = await _orders.CreateAsync(User.GetUserId(),
                model.Lines.Select(l => new LineInput(l.PhoneId, l.Amount)));
            TempData["Success"] = $"Заказ № {order.Id} оформлен и ожидает подтверждения.";
            return RedirectToAction(nameof(MyOrders));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("", ex.Message);
        }

        model.Lines = await LineResolver.BuildFormAsync(_db, model.Lines);
        return View(model);
    }

    // ---- мои заказы ----

    public async Task<IActionResult> MyOrders() =>
        View(new OrdersListViewModel { Orders = await _orders.GetForClientAsync(User.GetUserId()), SellerMode = false });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _orders.CancelAsync(id, User.GetUserId());
            TempData["Success"] = $"Заказ № {id} отменён.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(MyOrders));
    }
}
