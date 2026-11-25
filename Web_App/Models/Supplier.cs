using System.ComponentModel.DataAnnotations;

namespace Web_App.Models;

/// <summary>Поставщик — организация, осуществляющая поставку телефонов.</summary>
public class Supplier
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите наименование")]
    [StringLength(200)]
    [Display(Name = "Организация")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите e-mail")]
    [EmailAddress(ErrorMessage = "Некорректный e-mail")]
    [StringLength(200)]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    public override string ToString() => OrganizationName;
}
