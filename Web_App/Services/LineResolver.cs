using Microsoft.EntityFrameworkCore;
using Web_App.Data;
using Web_App.Models;
using Web_App.Models.ViewModels;

namespace Web_App.Services;

/// <summary>Позиция из формы: телефон и количество.</summary>
public record LineInput(int PhoneId, int Amount);

/// <summary>Общая обработка списка позиций (items_list) для заказа и продажи.</summary>
public static class LineResolver
{
    /// <summary>Проверяет позиции, отбрасывает нулевые, объединяет одинаковые телефоны.</summary>
    public static async Task<List<(Phone Phone, int Amount)>> ResolveAsync(ShopDbContext db, IEnumerable<LineInput> input)
    {
        var all = input.ToList();
        if (all.Any(l => l.Amount < 0))
            throw new BusinessException("Количество не может быть отрицательным.");

        var lines = all.Where(l => l.Amount > 0).ToList();
        if (lines.Count == 0)
            throw new BusinessException("Выберите хотя бы один телефон и укажите количество.");

        var grouped = lines.GroupBy(l => l.PhoneId)
            .Select(g => new { Id = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();
        var ids = grouped.Select(g => g.Id).ToList();
        var phones = await db.Phones.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var result = new List<(Phone Phone, int Amount)>();
        foreach (var g in grouped)
        {
            if (!phones.TryGetValue(g.Id, out var phone))
                throw new BusinessException("Один из выбранных телефонов не найден в каталоге.");
            result.Add((phone, g.Amount));
        }
        return result;
    }

    /// <summary>Строки формы выбора: весь каталог; введённые ранее количества сохраняются.</summary>
    public static async Task<List<ItemLine>> BuildFormAsync(ShopDbContext db, IEnumerable<ItemLine>? posted = null)
    {
        var amounts = (posted ?? Enumerable.Empty<ItemLine>())
            .GroupBy(l => l.PhoneId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var phones = await db.Phones.OrderBy(p => p.CompanyName).ThenBy(p => p.PhoneName).ToListAsync();
        return phones.Select(p => new ItemLine
        {
            PhoneId = p.Id,
            Title = p.ToString(),
            Price = p.Price,
            Stock = p.StockAmount,
            Amount = amounts.TryGetValue(p.Id, out var a) ? a : 0
        }).ToList();
    }
}
