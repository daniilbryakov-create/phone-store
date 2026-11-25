namespace Web_App.Models;

public enum UserRole
{
    Client,
    Seller,
    Manager,
    Admin
}

public enum OrderStatus
{
    Created,              // Оформлен
    AwaitingConfirmation, // Ожидание подтверждения
    Confirmed,            // Подтверждён
    AwaitingPayment,      // Ожидание оплаты
    Paid,                 // Оплачен
    ReadyForPickup,       // Готов к выдаче
    Completed,            // Выполнен
    Cancelled             // Отменён
}

public enum SupplyStatus
{
    Ordered,   // Оформлена
    Received,  // Получена
    Cancelled  // Отменена
}

public enum ReportType
{
    Sales,         // Продажи
    Supplies,      // Поставки
    Stock,         // Остатки
    SalesAndStock  // Продажи и остатки
}

public static class EnumExtensions
{
    public static string ToDisplayString(this OrderStatus status) => status switch
    {
        OrderStatus.Created => "Оформлен",
        OrderStatus.AwaitingConfirmation => "Ожидание подтверждения",
        OrderStatus.Confirmed => "Подтверждён",
        OrderStatus.AwaitingPayment => "Ожидание оплаты",
        OrderStatus.Paid => "Оплачен",
        OrderStatus.ReadyForPickup => "Готов к выдаче",
        OrderStatus.Completed => "Выполнен",
        OrderStatus.Cancelled => "Отменён",
        _ => status.ToString()
    };

    public static string ToDisplayString(this SupplyStatus status) => status switch
    {
        SupplyStatus.Ordered => "Оформлена",
        SupplyStatus.Received => "Получена",
        SupplyStatus.Cancelled => "Отменена",
        _ => status.ToString()
    };

    public static string ToDisplayString(this UserRole role) => role switch
    {
        UserRole.Client => "Клиент",
        UserRole.Seller => "Продавец",
        UserRole.Manager => "Менеджер",
        UserRole.Admin => "Администратор",
        _ => role.ToString()
    };
}
