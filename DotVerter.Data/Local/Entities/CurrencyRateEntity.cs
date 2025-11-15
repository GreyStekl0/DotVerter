using SQLite;

namespace DotVerter.Data.Local.Entities;

[Table("currency_rate")]
public sealed class CurrencyRateEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Indexed]
    [Column("requested_date")]
    public string RequestedDateKey { get; init; } = string.Empty;

    [Column("char_code")]
    public string CharCode { get; init; } = string.Empty;

    [Column("name")]
    public string Name { get; init; } = string.Empty;

    [Column("nominal")]
    public int Nominal { get; init; }

    [Column("rub_per_nominal")]
    public decimal RublesPerNominal { get; init; }
}
