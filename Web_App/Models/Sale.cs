using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_App.Models;

/// <summary>Продажа — зарегистрированный факт продажи.</summary>
public class Sale
{
    /// <summary>Срок гарантии, месяцев.</summary>
    public const int WarrantyMonths = 12;

    public int Id { get; set; }

    [Display(Name = "Дата")]
    public DateTime Date { get; set; }

    [Display(Name = "Итого, руб.")]
    public int FinalPrice { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int SellerId { get; set; }
    public Seller Seller { get; set; } = null!;

    public List<SaleItem> Items { get; set; } = new();

    [NotMapped]
    [Display(Name = "Гарантия до")]
    public DateTime WarrantyUntil => Date.AddMonths(WarrantyMonths);

    public int CalcSum() => Items.Sum(i => i.Amount * i.PricePerItem);
}

/// <summary>Предмет продажи.</summary>
public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int PhoneId { get; set; }
    public Phone Phone { get; set; } = null!;

    public int Amount { get; set; }
    public int PricePerItem { get; set; }

    /// <summary>Сколько единиц уже возвращено.</summary>
    public int ReturnedAmount { get; set; }

    [NotMapped]
    public int ReturnableAmount => Amount - ReturnedAmount;

    public List<Return> Returns { get; set; } = new();
}

/// <summary>Возврат по предмету продажи.</summary>
public class Return
{
    public int Id { get; set; }

    [Display(Name = "Дата")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Укажите причину возврата")]
    [StringLength(500)]
    [Display(Name = "Причина")]
    public string Reason { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Количество")]
    public int Amount { get; set; }

    [Display(Name = "Возвращено, руб.")]
    public int RefundSum { get; set; }

    public int SaleItemId { get; set; }
    public SaleItem SaleItem { get; set; } = null!;

    public int SellerId { get; set; }
    public Seller Seller { get; set; } = null!;
}
