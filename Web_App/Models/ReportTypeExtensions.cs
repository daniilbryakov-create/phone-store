namespace Web_App.Models;

public static class ReportTypeExtensions
{
    public static string ToDisplayString(this ReportType type) => type switch
    {
        ReportType.Sales => "Продажи",
        ReportType.Supplies => "Поставки",
        ReportType.Stock => "Остатки",
        ReportType.SalesAndStock => "Продажи и остатки",
        _ => type.ToString()
    };
}
