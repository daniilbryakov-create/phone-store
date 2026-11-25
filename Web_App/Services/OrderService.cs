using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Services;

/// <summary>Заказы: оформление клиентом и переходы по диаграмме состояний (рисунок 3).</summary>
public class OrderService
{
    private readonly ShopDbContext _db;
    public OrderService(ShopDbContext db) => _db = db;

    /// <summary>create_order(items_list): заказ получает статус «Ожидание подтверждения».</summary>
    public async Task<Order> CreateAsync(int clientId, IEnumerable<LineInput> lines)
    {
        var resolved = await LineResolver.ResolveAsync(_db, lines);
        var now = DateTime.Now;

        var order = new Order { Date = now, ClientId = clientId, Status = OrderStatus.AwaitingConfirmation };
        foreach (var (phone, amount) in resolved)
            order.Items.Add(new OrderItem { Phone = phone, Amount = amount, PricePerItem = phone.Price });

        order.TotalPrice = order.CalcSum();
        order.StatusHistory.Add(new OrderStatusChange { Status = OrderStatus.Created, Date = now });
        order.StatusHistory.Add(new OrderStatusChange { Status = OrderStatus.AwaitingConfirmation, Date = now });

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    /// <summary>Заказы клиента, от новых к старым.</summary>
    public Task<List<Order>> GetForClientAsync(int clientId) =>
        WithItems().Where(o => o.ClientId == clientId).OrderByDescending(o => o.Id).ToListAsync();

    /// <summary>Все заказы (для продавца), от новых к старым.</summary>
    public Task<List<Order>> GetAllAsync() =>
        WithItems().OrderByDescending(o => o.Id).ToListAsync();

    /// <summary>
    /// Подтверждение: если всё есть в наличии — товар резервируется и заказ ждёт оплаты (true);
    /// иначе заказ отменяется (false).
    /// </summary>
    public async Task<bool> ConfirmAsync(int id)
    {
        var order = await LoadAsync(id);
        Require(order, OrderStatus.AwaitingConfirmation);

        if (order.Items.All(i => i.Phone.AvailabilityCheck(i.Amount)))
        {
            foreach (var item in order.Items)
                item.Phone.StockAmount -= item.Amount;
            MoveTo(order, OrderStatus.Confirmed);
            MoveTo(order, OrderStatus.AwaitingPayment);
            await _db.SaveChangesAsync();
            return true;
        }

        MoveTo(order, OrderStatus.Cancelled);
        await _db.SaveChangesAsync();
        return false;
    }

    public async Task ConfirmPaymentAsync(int id)
    {
        var order = await LoadAsync(id);
        Require(order, OrderStatus.AwaitingPayment);
        MoveTo(order, OrderStatus.Paid);
        await _db.SaveChangesAsync();
    }

    public async Task MarkReadyForPickupAsync(int id)
    {
        var order = await LoadAsync(id);
        Require(order, OrderStatus.Paid);
        MoveTo(order, OrderStatus.ReadyForPickup);
        await _db.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id)
    {
        var order = await LoadAsync(id);
        Require(order, OrderStatus.ReadyForPickup);
        MoveTo(order, OrderStatus.Completed);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Отмена до получения оплаты; резерв возвращается на склад.
    /// Если задан clientId — клиент может отменять только свои заказы.
    /// </summary>
    public async Task CancelAsync(int id, int? onlyClientId = null)
    {
        var order = await LoadAsync(id);
        if (onlyClientId.HasValue && order.ClientId != onlyClientId.Value)
            throw new BusinessException("Заказ не найден.");

        switch (order.Status)
        {
            case OrderStatus.Created:
            case OrderStatus.AwaitingConfirmation:
                break;
            case OrderStatus.Confirmed:
            case OrderStatus.AwaitingPayment:
                foreach (var item in order.Items)
                    item.Phone.StockAmount += item.Amount;   // снимаем резерв
                break;
            default:
                throw new BusinessException(
                    $"Заказ в состоянии «{order.Status.ToDisplayString()}» отменить нельзя.");
        }
        MoveTo(order, OrderStatus.Cancelled);
        await _db.SaveChangesAsync();
    }

    // ---------- вспомогательное ----------

    private IQueryable<Order> WithItems() =>
        _db.Orders.Include(o => o.Client).Include(o => o.Items).ThenInclude(i => i.Phone);

    private async Task<Order> LoadAsync(int id) =>
        await WithItems().FirstOrDefaultAsync(o => o.Id == id)
        ?? throw new BusinessException("Заказ не найден.");

    private static void Require(Order order, OrderStatus expected)
    {
        if (order.Status != expected)
            throw new BusinessException(
                $"Операция недопустима: заказ в состоянии «{order.Status.ToDisplayString()}», " +
                $"а требуется «{expected.ToDisplayString()}».");
    }

    private static void MoveTo(Order order, OrderStatus status)
    {
        order.Status = status;
        order.StatusHistory.Add(new OrderStatusChange { Status = status, Date = DateTime.Now });
    }
}
