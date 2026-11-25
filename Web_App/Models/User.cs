using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_App.Models;

/// <summary>Пользователь. Все роли хранятся в одной таблице Users (TPH), колонка Discriminator.</summary>
public abstract class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите ФИО")]
    [StringLength(200)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Логин; у клиента, зарегистрированного продавцом офлайн, может отсутствовать.</summary>
    [StringLength(50)]
    [Display(Name = "Логин")]
    public string? Login { get; set; }

    /// <summary>Только PBKDF2-хэш, пароль в открытом виде не хранится.</summary>
    public string? PasswordHash { get; set; }

    [NotMapped]
    public abstract UserRole Kind { get; }

    [NotMapped]
    public bool HasAccount => !string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(PasswordHash);
}

/// <summary>Работник — родитель продавца, менеджера и администратора.</summary>
public abstract class Employee : User
{
    [NotMapped]
    public string Role => Kind.ToDisplayString();
}

public class Admin : Employee
{
    public override UserRole Kind => UserRole.Admin;
}

public class Manager : Employee
{
    public override UserRole Kind => UserRole.Manager;

    public List<Supply> Supplies { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
}

public class Seller : Employee
{
    public override UserRole Kind => UserRole.Seller;

    public List<Sale> Sales { get; set; } = new();
    public List<Return> Returns { get; set; } = new();
}

public class Client : User
{
    public override UserRole Kind => UserRole.Client;

    [Required(ErrorMessage = "Укажите номер телефона")]
    [StringLength(20)]
    [Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Некорректный e-mail")]
    [StringLength(200)]
    [Display(Name = "Электронная почта")]
    public string? Email { get; set; }

    public List<Order> Orders { get; set; } = new();
    public List<Sale> Sales { get; set; } = new();
}
