using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web_App.Models.ViewModels;

/// <summary>Строка формы поставки. Title/Price/Stock только для показа.</summary>
public class SupplyLine
{
    public int PhoneId { get; set; }
    public string? Title { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public int Amount { get; set; }
    public int PurchaseCost { get; set; }
}

public class SupplyCreateViewModel
{
    [Display(Name = "Поставщик")]
    public int SupplierId { get; set; }

    [Display(Name = "Сразу принять на склад")]
    public bool ReceiveNow { get; set; } = true;

    public List<SupplyLine> Lines { get; set; } = new();

    [BindNever]
    public List<Supplier> Suppliers { get; set; } = new();
}

public class ReportCreateViewModel
{
    [Display(Name = "Тип отчёта")]
    public ReportType Type { get; set; } = ReportType.SalesAndStock;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Дата начала")]
    public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(1 - DateTime.Today.Day);

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Дата окончания")]
    public DateTime DateTo { get; set; } = DateTime.Today;
}
