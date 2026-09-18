using Dashboard.Core.DTOs;

namespace Dashboard.Core.Contratos;

public interface IIndicatorRepository
{
    Task<IndicatorData> GetIndicatorAsync(IndicatorFilter filter, CancellationToken cancellationToken = default);
}