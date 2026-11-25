using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web_App.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите логин")]
    [Display(Name = "Логин")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }
}

public class PhoneSearchViewModel
{
    public string? Company { get; set; }
    public string? ModelName { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public bool OnlyAvailable { get; set; }

    [BindNever]
    public List<Phone> Results { get; set; } = new();
}

/// <summary>Строка формы выбора товара (заказ, продажа). Title/Price/Stock только для показа.</summary>
public class ItemLine
{
    public int PhoneId { get; set; }
    public string? Title { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public int Amount { get; set; }
}

public class OrderCreateViewModel
{
    public List<ItemLine> Lines { get; set; } = new();
}

public class SaleCreateViewModel
{
    public List<ItemLine> Lines { get; set; } = new();

    [Display(Name = "Телефон покупателя")]
    public string? ClientPhone { get; set; }

    [Display(Name = "ФИО (если клиента ещё нет в базе)")]
    public string? ClientName { get; set; }

    [Display(Name = "Электронная почта")]
    public string? ClientEmail { get; set; }
}

public class RegisterClientViewModel
{
    [Required(ErrorMessage = "Укажите ФИО")]
    [Display(Name = "ФИО")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Укажите телефон")]
    [Display(Name = "Телефон")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Электронная почта")]
    public string? Email { get; set; }
}

public class ReturnViewModel
{
    public int SaleItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
    [Display(Name = "Количество")]
    public int Amount { get; set; } = 1;

    [Required(ErrorMessage = "Укажите причину возврата")]
    [Display(Name = "Причина возврата")]
    public string? Reason { get; set; }

    // только для показа
    public int SaleId { get; set; }
    public string? PhoneTitle { get; set; }
    public int ReturnableAmount { get; set; }
    public int PricePerItem { get; set; }
}

public class OrdersListViewModel
{
    public List<Order> Orders { get; set; } = new();
    public bool SellerMode { get; set; }
}

public class UserCreateViewModel
{
    [Display(Name = "Роль")]
    public UserRole Role { get; set; } = UserRole.Seller;

    [Required(ErrorMessage = "Укажите ФИО")]
    [Display(Name = "ФИО")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Укажите логин")]
    [Display(Name = "Логин")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Укажите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Display(Name = "Телефон (для покупателя)")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Электронная почта (для покупателя)")]
    public string? Email { get; set; }
}

public class UserEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "ФИО")]
    public string? FullName { get; set; }

    [Display(Name = "Логин")]
    public string? Login { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль (пусто — не менять)")]
    public string? Password { get; set; }

    // только для показа
    public string? RoleTitle { get; set; }
}
