using System.Data;
using Dapper;
using Dashboard.Core.DTOs;
using Dashboard.Data.Connections;

namespace Dashboard.Data.Repositories;

public sealed class FaturamentoRepository(ISqlConnectionFactory connectionFactory)
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    internal const string Sql = """
        SELECT SUM((F.Valor + F.CustoOP + F.Filme) * F.Quant)
        FROM dbo.ENTRADA E
        LEFT JOIN dbo.CADMEDICO M
               ON M.CodMedico = E.CodMedico
              AND M.GrupoEmp = E.GrupoEmp
              AND M.Filial = E.Filial
        LEFT JOIN dbo.CADCONVENIO C
               ON C.CodConvenio = E.CodConvenio
              AND C.GrupoEmp = E.GrupoEmp
              AND C.Filial = E.Filial
        LEFT JOIN dbo.FATURA F
               ON F.CodPaciente = E.CodMovimento
        WHERE E.Tipo IN ('1','3','4','5','6','7')
          AND E.Fechado IN ('F','E')
          AND E.LoteEnt <> 'INAT'
          AND E.Restrito <> 'Z'
          AND E.GrupoEmp = '01'
          AND E.Filial = '01'
          AND C.ModoFat IS NOT NULL
          AND E.Guia <> 'GUIA MEDICO'
          AND E.Guia <> 'LENTEC'
          AND M.REDUZIDO IS NOT NULL
          AND E.DataHoraEnt >= @DataInicial
          AND E.DataHoraEnt < @DataFinalExclusiva
        """;

    public async Task<FaturamentoDto> ObterFaturamentoRealizadoAsync(
        DateOnly dataInicial,
        DateOnly dataFinal,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dataInicial, dataFinal, nameof(dataInicial));

        var parametros = new DynamicParameters();
        parametros.Add("DataInicial", dataInicial.ToDateTime(TimeOnly.MinValue));
        parametros.Add("DataFinalExclusiva", dataFinal.AddDays(1).ToDateTime(TimeOnly.MinValue));

        using IDbConnection connection = _connectionFactory.Create();

        var command = new CommandDefinition(Sql, parametros, cancellationToken: cancellationToken);
        decimal? total = await connection.ExecuteScalarAsync<decimal?>(command);

        return new FaturamentoDto
        {
            DataInicial = dataInicial,
            DataFinal = dataFinal,
            ValorFaturado = total ?? 0m,
        };
    }
}
