namespace Dashboard.Core.DTOs;

public sealed class FaturamentoDto
{
    public DateOnly DataInicial { get; set; }

    public DateOnly DataFinal { get; set; }

    public decimal ValorFaturado { get; set; }
}
