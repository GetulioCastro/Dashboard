namespace Dashboard.Core.DTOs;

public enum PeriodType
{
    Current,
    Past,
    Future
}

public enum CoverageCategory
{
    Particular,
    Convenio,
    Sus
}

public enum TipoAtendimento
{
    Consulta,
    Retorno,
    Exame,
    PequenoProcedimento,
    Clinico,
    Cirurgia,
    NaoClassificado
}

public sealed class IndicatorFilter
{
    public PeriodType Period { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public CoverageCategory? Coverage { get; set; }

    public TipoAtendimento? Tipo { get; set; }
}