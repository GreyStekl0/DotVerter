using SQLite;

namespace DotVerter.Data.Local.Entities;

[Table("rates_for_date")]
public sealed class RatesForDateEntity
{
    [PrimaryKey]
    [Column("requested_date")]
    public string RequestedDateKey { get; init; } = string.Empty;

    [Column("actual_date")]
    public string ActualDateKey { get; init; } = string.Empty;
}
