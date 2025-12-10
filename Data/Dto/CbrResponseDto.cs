namespace Data.Dto;

/// <summary>
///     DTO ответа от API ЦБ РФ
/// </summary>
public class CbrResponseDto
{
    public DateTime Date { get; set; }
    public DateTime PreviousDate { get; set; }
    public string PreviousURL { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, CurrencyDto> Valute { get; set; } = new();
    
    /// <summary>
    ///     Дата из URL архивного запроса (для корректной работы с часовыми поясами)
    /// </summary>
    public DateOnly? RequestedArchiveDate { get; set; }
}

/// <summary>
///     DTO валюты от API ЦБ РФ
/// </summary>
public class CurrencyDto
{
    public string ID { get; set; } = string.Empty;
    public string NumCode { get; set; } = string.Empty;
    public string CharCode { get; set; } = string.Empty;
    public int Nominal { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Previous { get; set; }
}