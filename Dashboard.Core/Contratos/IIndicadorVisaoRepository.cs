using Dashboard.Core.DTOs;

namespace Dashboard.Core.Contratos;

public interface IIndicadorVisaoRepository
{
    Task<IndicadorVisao> ObterVisaoAsync(
        IndicatorFilter filter,
        DimensaoVisao dimensao,
        GraficoForma forma,
        CancellationToken cancellationToken = default);
}
