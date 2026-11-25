using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_App.Models;

/// <summary>Строка отчёта о продажах.</summary>
public class SoldLine
{
    public string Phone { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Revenue { get; set; }
}

/// <summary>Строка отчёта об остатках.</summary>
public class StockLine
{
    public string Phone { get; set; } = string.Empty;
    public int Amount { get; set; }
}

/// <summary>Отчёт менеджера за период (границы включаются). Строки хранятся в колонках JSON.</summary>
public class Report
{
    public int Id { get; set; }

    [Display(Name = "Тип")]
    public ReportType Type { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "С")]
    public DateTime DateFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "По")]
    public DateTime DateTo { get; set; }

    [Display(Name = "Сформирован")]
    public DateTime CreatedAt { get; set; }

    public int ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;

    // продажи
    public int SalesCount { get; set; }
    public int SalesRevenue { get; set; }
    public int ReturnsCount { get; set; }
    public int RefundsTotal { get; set; }

    [NotMapped]
    public int NetRevenue => SalesRevenue - RefundsTotal;

    public List<SoldLine> SoldLines { get; set; } = new();

    // поставки (отменённые не учитываются)
    public int SuppliesCount { get; set; }
    public int PurchaseCost { get; set; }

    // остатки на момент формирования
    public List<StockLine> StockLines { get; set; } = new();
}
