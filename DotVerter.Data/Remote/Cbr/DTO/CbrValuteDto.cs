namespace DotVerter.Data.Remote.Cbr.DTO;

public sealed class CbrValuteDto
{
    public string ID { get; init; } = null!;
    public string NumCode { get; init; } = null!;
    public string CharCode { get; init; } = null!;
    public int Nominal { get; init; }
    public string Name { get; init; } = null!;
    public decimal Value { get; init; }
    public decimal Previous { get; init; }
}