namespace Web_App.Services;

/// <summary>Нарушение бизнес-правила: текст сообщения показывается пользователю.</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
