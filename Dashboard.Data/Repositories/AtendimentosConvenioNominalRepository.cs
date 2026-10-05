using System.Data;
using Dapper;
using Dashboard.Core.Contratos;
using Dashboard.Core.DTOs;
using Dashboard.Core.Regras;
using Dashboard.Data.Connections;

namespace Dashboard.Data.Repositories;

public sealed class AtendimentosConvenioNominalRepository(ISqlConnectionFactory connectionFactory) : IIndicadorConvenioNominalRepository
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IndicadorConvenioNominalVisao> ObterConvenioNominalAsync(
        IndicatorFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        BusinessReferencePeriod period = PeriodoResolutor.Resolver(filter);
        DateTime inicio = period.Start.ToDateTime(TimeOnly.MinValue);
        DateTime fim = period.End.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var parametros = new DynamicParameters();
        parametros.Add("DataInicio", inicio);
        parametros.Add("DataFim", fim);

        const string sql = @"SELECT 
                C.DESCR AS Identificacao,
                COUNT(1) AS Volume
            FROM dbo.ENTRADA E
            INNER JOIN dbo.CADCONVENIO C ON C.CODCONVENIO = E.CODCONVENIO AND C.GRUPOEMP = E.GRUPOEMP AND C.FILIAL = E.FILIAL
            WHERE E.DATAHORAENT >= @DataInicio
              AND E.DATAHORAENT < @DataFim
              AND E.FECHADO <> 'C'
              AND COALESCE(E.LoteEnt,'') <> 'INAT'
              AND COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'
            GROUP BY C.CODCONVENIO, C.GRUPOEMP, C.FILIAL, C.DESCR
            ORDER BY COUNT(1) DESC, C.DESCR ASC, C.CODCONVENIO ASC, C.GRUPOEMP ASC, C.FILIAL ASC";

        using IDbConnection connection = _connectionFactory.Create();
        var command = new CommandDefinition(sql, parametros, cancellationToken: cancellationToken);
        var linhas = (await connection.QueryAsync<ConvenioNominalVolume>(command)).ToList();

        decimal total = linhas.Sum(l => l.Volume);
        return new IndicadorConvenioNominalVisao
        {
            Periodo = period,
            ValorTotal = total,
            Convenios = linhas,
        };
    }
}
