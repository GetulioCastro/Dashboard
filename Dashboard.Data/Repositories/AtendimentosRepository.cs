using System.Data;
using Dapper;
using Dashboard.Core.Contratos;
using Dashboard.Core.DTOs;
using Dashboard.Core.Regras;
using Dashboard.Data.Connections;

namespace Dashboard.Data.Repositories;

public sealed class AtendimentosRepository(ISqlConnectionFactory connectionFactory) : IIndicatorRepository
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    private static readonly IReadOnlyDictionary<TipoAtendimento, string> TipoParaCodigo = new Dictionary<TipoAtendimento, string>
    {
        [TipoAtendimento.Consulta] = "1",
        [TipoAtendimento.Retorno] = "2",
        [TipoAtendimento.Exame] = "3",
        [TipoAtendimento.PequenoProcedimento] = "4",
        [TipoAtendimento.Clinico] = "5",
        [TipoAtendimento.Cirurgia] = "6",
    };

    private static readonly IReadOnlyDictionary<CoverageCategory, string> CoberturaParaModo = new Dictionary<CoverageCategory, string>
    {
        [CoverageCategory.Particular] = "P",
        [CoverageCategory.Convenio] = "C",
        [CoverageCategory.Sus] = "S",
    };

    internal static string ConstruirClausulaFrom(bool comCobertura) =>
        comCobertura
            ? "dbo.ENTRADA E INNER JOIN dbo.CADCONVENIO C ON C.CODCONVENIO = E.CODCONVENIO AND C.GRUPOEMP = E.GRUPOEMP AND C.FILIAL = E.FILIAL"
            : "dbo.ENTRADA E";

    internal static IReadOnlyList<string> ConstruirCondicoesCobertura(CoverageCategory cobertura) =>
        cobertura == CoverageCategory.Convenio
            ? new[] { "C.MODOFAT = @ModoFatura", "COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'" }
            : new[] { "C.MODOFAT = @ModoFatura" };

    public async Task<IndicatorData> GetIndicatorAsync(IndicatorFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        BusinessReferencePeriod period = PeriodoResolutor.Resolver(filter);

        var conditions = new List<string>
        {
            "E.DATAHORAENT >= @DataInicio",
            "E.DATAHORAENT < @DataFim",
            "E.FECHADO <> 'C'",
            "E.LoteEnt <> 'INAT'",
        };

        var parametros = new DynamicParameters();
        parametros.Add("DataInicio", period.Start.ToDateTime(TimeOnly.MinValue));
        parametros.Add("DataFim", period.End.AddDays(1).ToDateTime(TimeOnly.MinValue));

        if (filter.Tipo.HasValue)
        {
            if (filter.Tipo.Value == TipoAtendimento.NaoClassificado)
            {
                conditions.Add("(E.TIPO IS NULL OR E.TIPO NOT IN ('1','2','3','4','5','6'))");
            }
            else
            {
                conditions.Add("E.TIPO = @Tipo");
                parametros.Add("Tipo", TipoParaCodigo[filter.Tipo.Value]);
            }
        }

        if (filter.Coverage.HasValue)
        {
            conditions.AddRange(ConstruirCondicoesCobertura(filter.Coverage.Value));
            parametros.Add("ModoFatura", CoberturaParaModo[filter.Coverage.Value]);
        }

        string from = ConstruirClausulaFrom(filter.Coverage.HasValue);

        string sql = $"SELECT COUNT(1) FROM {from} WHERE {string.Join(" AND ", conditions)}";

        using IDbConnection connection = _connectionFactory.Create();

        var command = new CommandDefinition(sql, parametros, cancellationToken: cancellationToken);
        int quantidade = await connection.ExecuteScalarAsync<int>(command);

        return new IndicatorData
        {
            Value = quantidade,
            ReferencePeriod = period,
            UnitOfMeasure = "atendimentos",
        };
    }
}