using System.Text.Json.Serialization;

namespace DotVerter.Data.Remote.Cbr.DTO;

public sealed class CbrDailyResponseDto
{
    public DateTime Date { get; init; }
    public DateTime PreviousDate { get; init; }
    [JsonPropertyName("PreviousURL")]
    public string PreviousUrl { get; init; } = null!;
    public DateTime Timestamp { get; init; }

    public Dictionary<string, CbrValuteDto> Valute { get; init; } = new();
}