using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Services;

/// <summary>Позиция поставки из формы: телефон, количество, закупочная цена за единицу.</summary>
public record SupplyLineInput(int PhoneId, int Amount, int PurchaseCost);

/// <summary>Менеджер: оформление поставок и управление ими (таблица 3).</summary>
public class SupplyService
{
    private readonly ShopDbContext _db;
    public SupplyService(ShopDbContext db) => _db = db;

    /// <summary>
    /// order_phones(supplier, phones_list): receiveNow = true — поставка сразу принимается на склад
    /// (остатки растут), иначе остаётся «Оформлена».
    /// </summary>
    public async Task<Supply> CreateAsync(int managerId, int supplierId, IEnumerable<SupplyLineInput> input, bool receiveNow)
    {
        if (!await _db.Suppliers.AnyAsync(s => s.Id == supplierId))
            throw new BusinessException("Выберите поставщика.");

        var all = input.ToList();
        if (all.Any(l => l.Amount < 0))
            throw new BusinessException("Количество не может быть отрицательным.");

        var lines = all.Where(l => l.Amount > 0).ToList();
        if (lines.Count == 0)
            throw new BusinessException("Список телефонов в поставке пуст: укажите количество хотя бы для одного телефона.");
        if (lines.Any(l => l.PurchaseCost <= 0))
            throw new BusinessException("Укажите закупочную стоимость за единицу для каждого выбранного телефона.");

        var ids = lines.Select(l => l.PhoneId).Distinct().ToList();
        var phones = await _db.Phones.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var supply = new Supply
        {
            Date = DateTime.Now,
            SupplierId = supplierId,
            ManagerId = managerId,
            Status = SupplyStatus.Ordered
        };
        foreach (var line in lines)
        {
            if (!phones.TryGetValue(line.PhoneId, out var phone))
                throw new BusinessException("Модель отсутствует в каталоге: сначала добавьте её в каталог.");
            supply.Items.Add(new SupplyItem { Phone = phone, Amount = line.Amount, PurchaseCost = line.PurchaseCost });
        }
        supply.TotalPurchaseCost = supply.Items.Sum(i => i.Amount * i.PurchaseCost);

        if (receiveNow)
            Receive(supply);

        _db.Supplies.Add(supply);
        await _db.SaveChangesAsync();
        return supply;
    }

    /// <summary>update_storage(): принять оформленную поставку — остатки растут.</summary>
    public async Task ReceiveAsync(int id)
    {
        var supply = await LoadAsync(id);
        Receive(supply);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Отмена поставки. Принятую поставку можно отменить, пока принятый товар не распродан:
    /// остатки уменьшаются обратно (либо отмена полная, либо никакая).
    /// </summary>
    public async Task CancelAsync(int id)
    {
        var supply = await LoadAsync(id);
        switch (supply.Status)
        {
            case SupplyStatus.Ordered:
                break;

            case SupplyStatus.Received:
                var needed = supply.Items
                    .GroupBy(i => i.PhoneId)
                    .Select(g => new { Phone = g.First().Phone, Amount = g.Sum(x => x.Amount) })
                    .ToList();
                foreach (var n in needed)
                {
                    if (!n.Phone.AvailabilityCheck(n.Amount))
                        throw new BusinessException(
                            $"Поставку нельзя отменить: товар «{n.Phone}» уже частично продан " +
                            $"(на складе {n.Phone.StockAmount} шт., а в поставке {n.Amount} шт.).");
                }
                foreach (var item in supply.Items)
                    item.Phone.StockAmount -= item.Amount;
                break;

            default:
                throw new BusinessException("Поставка уже отменена.");
        }
        supply.Status = SupplyStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    public Task<List<Supply>> ListAsync() =>
        _db.Supplies
            .Include(s => s.Supplier).Include(s => s.Manager)
            .Include(s => s.Items).ThenInclude(i => i.Phone)
            .OrderByDescending(s => s.Id)
            .ToListAsync();

    private static void Receive(Supply supply)
    {
        if (supply.Status != SupplyStatus.Ordered)
            throw new BusinessException(
                $"Принять можно только оформленную поставку, а эта в состоянии «{supply.Status.ToDisplayString()}».");

        foreach (var item in supply.Items)
            item.Phone.StockAmount += item.Amount;
        supply.Status = SupplyStatus.Received;
    }

    private async Task<Supply> LoadAsync(int id) =>
        await _db.Supplies.Include(s => s.Items).ThenInclude(i => i.Phone).FirstOrDefaultAsync(s => s.Id == id)
        ?? throw new BusinessException("Поставка не найдена.");
}
