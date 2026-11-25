using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Services;

/// <summary>Продавец: продажи, возвраты, регистрация клиентов (рисунки 4 и 5).</summary>
public class SaleService
{
    private readonly ShopDbContext _db;
    public SaleService(ShopDbContext db) => _db = db;

    /// <summary>
    /// sell_phones(items_list, client): проверка наличия → поиск/регистрация клиента →
    /// создание продажи → списание остатков. Всё сохраняется одним SaveChanges («всё или ничего»).
    /// </summary>
    public async Task<Sale> SellAsync(int sellerId, IEnumerable<LineInput> lines,
        string? clientPhone, string? clientName, string? clientEmail)
    {
        var resolved = await LineResolver.ResolveAsync(_db, lines);

        foreach (var (phone, amount) in resolved)
        {
            if (!phone.AvailabilityCheck(amount))
                throw new BusinessException(
                    $"Недостаточно товара «{phone}» на складе: нужно {amount} шт., есть {phone.StockAmount} шт.");
        }

        var number = Rules.PhoneNumber(clientPhone);
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == number);
        if (client is null)
        {
            client = new Client
            {
                FullName = Rules.NotBlank(clientName, "ФИО покупателя"),
                PhoneNumber = number,
                Email = Rules.OptionalEmail(clientEmail)
            };
            _db.Clients.Add(client);
        }

        var sale = new Sale { Date = DateTime.Now, Client = client, SellerId = sellerId };
        foreach (var (phone, amount) in resolved)
            sale.Items.Add(new SaleItem { Phone = phone, Amount = amount, PricePerItem = phone.Price });
        sale.FinalPrice = sale.CalcSum();

        foreach (var (phone, amount) in resolved)
            phone.StockAmount -= amount;

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();
        return sale;
    }

    /// <summary>register_client(full_name, phone_number, email) — клиент без учётной записи.</summary>
    public async Task<Client> RegisterClientAsync(string? fullName, string? phoneNumber, string? email)
    {
        var name = Rules.NotBlank(fullName, "ФИО");
        var number = Rules.PhoneNumber(phoneNumber);
        var mail = Rules.OptionalEmail(email);

        if (await _db.Clients.AnyAsync(c => c.PhoneNumber == number))
            throw new BusinessException($"Клиент с номером {number} уже зарегистрирован.");

        var client = new Client { FullName = name, PhoneNumber = number, Email = mail };
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return client;
    }

    /// <summary>process_return(sale_item, amount, reason): товар возвращается на склад.</summary>
    public async Task<Return> ReturnAsync(int sellerId, int saleItemId, int amount, string? reason)
    {
        var item = await _db.SaleItems.Include(i => i.Phone).FirstOrDefaultAsync(i => i.Id == saleItemId)
                   ?? throw new BusinessException("Позиция продажи не найдена.");

        if (amount <= 0)
            throw new BusinessException("Количество к возврату должно быть больше нуля.");
        var cleanReason = Rules.NotBlank(reason, "Причина возврата", 500);
        if (amount > item.ReturnableAmount)
            throw new BusinessException(
                $"Нельзя вернуть {amount} шт.: по этой позиции можно вернуть не более {item.ReturnableAmount} шт.");

        var ret = new Return
        {
            Date = DateTime.Now,
            Reason = cleanReason,
            Amount = amount,
            RefundSum = amount * item.PricePerItem,
            SaleItemId = item.Id,
            SellerId = sellerId
        };
        item.ReturnedAmount += amount;
        item.Phone.StockAmount += amount;

        _db.Returns.Add(ret);
        await _db.SaveChangesAsync();
        return ret;
    }

    /// <summary>view_sales_history() — от новых к старым.</summary>
    public Task<List<Sale>> HistoryAsync() =>
        WithDetails().OrderByDescending(s => s.Date).ThenByDescending(s => s.Id).ToListAsync();

    public Task<Sale?> GetAsync(int id) => WithDetails().FirstOrDefaultAsync(s => s.Id == id);

    public Task<SaleItem?> GetItemAsync(int saleItemId) =>
        _db.SaleItems.Include(i => i.Phone).FirstOrDefaultAsync(i => i.Id == saleItemId);

    private IQueryable<Sale> WithDetails() =>
        _db.Sales.Include(s => s.Client).Include(s => s.Seller)
                 .Include(s => s.Items).ThenInclude(i => i.Phone);
}
