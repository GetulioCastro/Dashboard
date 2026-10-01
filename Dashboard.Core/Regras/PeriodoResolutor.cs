using Dashboard.Core.DTOs;

namespace Dashboard.Core.Regras;

public static class PeriodoResolutor
{
    public static BusinessReferencePeriod Resolver(IndicatorFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return Resolver(filter, DateOnly.FromDateTime(DateTime.Today));
    }

    public static BusinessReferencePeriod Resolver(IndicatorFilter filter, DateOnly hoje)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return (filter.Period, filter.StartDate, filter.EndDate) switch
        {
            (PeriodType.Current, _, _) => MesCorrente(hoje),
            (PeriodType.Past, DateOnly inicio, DateOnly fim) when inicio <= fim => new BusinessReferencePeriod(inicio, fim),
            (PeriodType.Future, DateOnly inicio, DateOnly fim) when inicio <= fim => new BusinessReferencePeriod(inicio, fim),
            (PeriodType.Past, _, _) or (PeriodType.Future, _, _) => throw new ArgumentException(
                $"O período {filter.Period} exige StartDate e EndDate com início menor ou igual ao fim (RN-04, RN-42).",
                nameof(filter)),
            _ => throw new ArgumentOutOfRangeException(nameof(filter)),
        };
    }

    private static BusinessReferencePeriod MesCorrente(DateOnly data)
    {
        DateOnly inicio = new(data.Year, data.Month, 1);
        DateOnly fim = inicio.AddMonths(1).AddDays(-1);
        return new BusinessReferencePeriod(inicio, fim);
    }
}