namespace Dashboard.Core.DTOs;

public readonly record struct BusinessReferencePeriod(DateOnly Start, DateOnly End);

public sealed class IndicatorData
{
    public decimal Value { get; set; }

    public BusinessReferencePeriod ReferencePeriod { get; set; } = new();

    public string UnitOfMeasure { get; set; } = string.Empty;
}