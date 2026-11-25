using System.ComponentModel.DataAnnotations;

namespace Web_App.Models;

/// <summary>Поставка — поступление телефонов от поставщика.</summary>
public class Supply
{
    public int Id { get; set; }

    [Display(Name = "Дата")]
    public DateTime Date { get; set; }

    [Display(Name = "Статус")]
    public SupplyStatus Status { get; set; } = SupplyStatus.Ordered;

    [Display(Name = "Закупка, руб.")]
    public int TotalPurchaseCost { get; set; }

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public int ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;

    public List<SupplyItem> Items { get; set; } = new();
}

/// <summary>Предмет поставки: количество и закупочная цена за единицу.</summary>
public class SupplyItem
{
    public int Id { get; set; }

    public int SupplyId { get; set; }
    public Supply Supply { get; set; } = null!;

    public int PhoneId { get; set; }
    public Phone Phone { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Amount { get; set; }

    [Range(1, int.MaxValue)]
    public int PurchaseCost { get; set; }
}
