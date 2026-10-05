using Dashboard.Core.DTOs;

namespace Dashboard.Core.Contratos;

public interface IIndicadorConvenioNominalRepository
{
    Task<IndicadorConvenioNominalVisao> ObterConvenioNominalAsync(
        IndicatorFilter filter,
        CancellationToken cancellationToken = default);
}
