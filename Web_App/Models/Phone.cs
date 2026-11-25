using System.ComponentModel.DataAnnotations;

namespace Web_App.Models;

/// <summary>Телефон — товар магазина.</summary>
public class Phone
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название")]
    [StringLength(100)]
    [Display(Name = "Название")]
    public string PhoneName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите производителя")]
    [StringLength(100)]
    [Display(Name = "Производитель")]
    public string CompanyName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
    [Display(Name = "Цена, руб.")]
    public int Price { get; set; }

    [Required(ErrorMessage = "Укажите характеристики")]
    [StringLength(1000)]
    [Display(Name = "Характеристики")]
    public string Specification { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Остаток не может быть отрицательным")]
    [Display(Name = "На складе")]
    public int StockAmount { get; set; }

    public bool AvailabilityCheck(int quantity = 1) => quantity > 0 && StockAmount >= quantity;

    public override string ToString() => $"{CompanyName} {PhoneName}";
}
