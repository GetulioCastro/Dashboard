namespace Dashboard.Core.DTOs;

public sealed class ConvenioNominalVolume
{
    public string Identificacao { get; set; } = string.Empty;

    public decimal Volume { get; set; }
}

public sealed class IndicadorConvenioNominalVisao
{
    public BusinessReferencePeriod Periodo { get; set; }

    public decimal ValorTotal { get; set; }

    public List<ConvenioNominalVolume> Convenios { get; set; } = [];
}
