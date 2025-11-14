namespace DotVerter.Domain.Models;

public readonly record struct Money(decimal Amount, Currency Currency);