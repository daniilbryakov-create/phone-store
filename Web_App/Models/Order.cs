using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_App.Models;

/// <summary>Заказ клиента. Допустимые переходы статусов проверяет OrderService.</summary>
public class Order
{
    public int Id { get; set; }

    [Display(Name = "Дата")]
    public DateTime Date { get; set; }

    [Display(Name = "Статус")]
    public OrderStatus Status { get; set; }

    [Display(Name = "Сумма, руб.")]
    public int TotalPrice { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public List<OrderItem> Items { get; set; } = new();

    /// <summary>Состояния, через которые прошёл заказ (диаграмма состояний).</summary>
    public List<OrderStatusChange> StatusHistory { get; set; } = new();

    public int CalcSum() => Items.Sum(i => i.Amount * i.PricePerItem);
}

/// <summary>Предмет заказа: количество и цена за единицу на момент оформления.</summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int PhoneId { get; set; }
    public Phone Phone { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Amount { get; set; }

    public int PricePerItem { get; set; }
}

/// <summary>Запись истории статусов заказа.</summary>
public class OrderStatusChange
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public DateTime Date { get; set; }
}
