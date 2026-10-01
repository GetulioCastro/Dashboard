namespace Dashboard.Core.DTOs;

public enum DimensaoVisao
{
    Nenhuma,
    Convenio,
    Sexo
}

public enum GraficoForma
{
    Linha,
    Coluna,
    Donut
}

public sealed class PontoGrafico
{
    public DateOnly Data { get; set; }

    public string Rotulo { get; set; } = string.Empty;

    public decimal Valor { get; set; }
}

public sealed class SerieGrafico
{
    public string Nome { get; set; } = string.Empty;

    public string Cor { get; set; } = string.Empty;

    public List<PontoGrafico> Pontos { get; set; } = [];
}

public sealed class IndicadorVisao
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Unidade { get; set; } = string.Empty;

    public bool Monetario { get; set; }

    public BusinessReferencePeriod Periodo { get; set; }

    public decimal ValorTotal { get; set; }

    public GraficoForma Forma { get; set; }

    public DimensaoVisao DimensaoAtiva { get; set; }

    public List<DimensaoVisao> DimensoesDisponiveis { get; set; } = [];

    public List<SerieGrafico> Series { get; set; } = [];
}