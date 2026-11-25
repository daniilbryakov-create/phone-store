using System.Text.RegularExpressions;

namespace Web_App.Services;

/// <summary>Проверки данных (перенесены из Check.cs консольной версии).</summary>
public static class Rules
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static string NotBlank(string? value, string field, int maxLength = 200)
    {
        var t = value?.Trim();
        if (string.IsNullOrEmpty(t))
            throw new BusinessException($"Поле «{field}» не заполнено.");
        if (t.Length > maxLength)
            throw new BusinessException($"Поле «{field}» не должно быть длиннее {maxLength} символов.");
        return t;
    }

    /// <summary>Пустая почта допустима (null), непустая должна быть корректной.</summary>
    public static string? OptionalEmail(string? value)
    {
        var t = value?.Trim();
        if (string.IsNullOrEmpty(t)) return null;
        if (!EmailRegex.IsMatch(t))
            throw new BusinessException("Электронная почта указана в неверном формате.");
        return t;
    }

    /// <summary>Приводит номер к виду «+79001234567». 8XXXXXXXXXX и 10 цифр считаются российскими номерами.</summary>
    public static string PhoneNumber(string? value)
    {
        var raw = NotBlank(value, "Номер телефона", 30);
        if (raw.Any(c => !(char.IsDigit(c) || " +-()".Contains(c))))
            throw new BusinessException("Номер телефона может содержать только цифры, пробелы и символы + - ( ).");

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits[0] == '8')
            digits = "7" + digits[1..];
        else if (digits.Length == 10)
            digits = "7" + digits;

        if (digits.Length < 11 || digits.Length > 15)
            throw new BusinessException("Номер телефона должен содержать от 10 до 15 цифр.");
        return "+" + digits;
    }

    public static string Login(string? value)
    {
        var t = NotBlank(value, "Логин", 32);
        if (t.Length < 3)
            throw new BusinessException("Логин должен содержать не менее 3 символов.");
        if (t.Any(char.IsWhiteSpace))
            throw new BusinessException("Логин не должен содержать пробелов.");
        return t;
    }

    public static string Password(string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new BusinessException("Поле «Пароль» не заполнено.");
        if (value.Length < 6)
            throw new BusinessException("Пароль должен содержать не менее 6 символов.");
        if (value.Length > 128)
            throw new BusinessException("Пароль не должен быть длиннее 128 символов.");
        return value;
    }
}
