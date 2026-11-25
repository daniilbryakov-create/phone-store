using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;

namespace Web_App.Services;

/// <summary>Менеджер: формирование отчётов за период (границы включаются).</summary>
public class ReportService
{
    private readonly ShopDbContext _db;
    public ReportService(ShopDbContext db) => _db = db;

    /// <summary>create_report(date_from, date_to).</summary>
    public async Task<Report> CreateAsync(int managerId, DateTime dateFrom, DateTime dateTo, ReportType type)
    {
        var from = dateFrom.Date;
        var to = dateTo.Date;
        if (from > to)
            throw new BusinessException("Дата начала периода не может быть позже даты окончания.");
        var end = to.AddDays(1);   // [from, end) — весь последний день включается

        var report = new Report
        {
            Type = type,
            DateFrom = from,
            DateTo = to,
            CreatedAt = DateTime.Now,
            ManagerId = managerId
        };

        if (type is ReportType.Sales or ReportType.SalesAndStock)
        {
            var sales = await _db.Sales
                .Include(s => s.Items).ThenInclude(i => i.Phone)
                .Where(s => s.Date >= from && s.Date < end)
                .ToListAsync();
            var returns = await _db.Returns
                .Where(r => r.Date >= from && r.Date < end)
                .ToListAsync();

            report.SalesCount = sales.Count;
            report.SalesRevenue = sales.Sum(s => s.FinalPrice);
            report.ReturnsCount = returns.Count;
            report.RefundsTotal = returns.Sum(r => r.RefundSum);
            report.SoldLines = sales
                .SelectMany(s => s.Items)
                .GroupBy(i => i.PhoneId)
                .Select(g => new SoldLine
                {
                    Phone = g.First().Phone.ToString(),
                    Quantity = g.Sum(i => i.Amount),
                    Revenue = g.Sum(i => i.Amount * i.PricePerItem)
                })
                .OrderByDescending(l => l.Revenue)
                .ThenBy(l => l.Phone, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (type == ReportType.Supplies)
        {
            // отменённые поставки не учитываются
            var supplies = await _db.Supplies
                .Where(s => s.Status != SupplyStatus.Cancelled && s.Date >= from && s.Date < end)
                .ToListAsync();
            report.SuppliesCount = supplies.Count;
            report.PurchaseCost = supplies.Sum(s => s.TotalPurchaseCost);
        }

        if (type is ReportType.Stock or ReportType.SalesAndStock)
        {
            var phones = await _db.Phones.OrderBy(p => p.CompanyName).ThenBy(p => p.PhoneName).ToListAsync();
            report.StockLines = phones
                .Select(p => new StockLine { Phone = p.ToString(), Amount = p.StockAmount })
                .ToList();
        }

        _db.Reports.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    public Task<List<Report>> ListAsync() =>
        _db.Reports.Include(r => r.Manager).OrderByDescending(r => r.Id).ToListAsync();

    public Task<Report?> GetAsync(int id) =>
        _db.Reports.Include(r => r.Manager).FirstOrDefaultAsync(r => r.Id == id);
}
