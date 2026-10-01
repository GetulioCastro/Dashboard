using System.Data;
using System.Globalization;
using Dapper;
using Dashboard.Core.Contratos;
using Dashboard.Core.DTOs;
using Dashboard.Core.Regras;
using Dashboard.Data.Connections;

namespace Dashboard.Data.Repositories;

public sealed class ExamesVisaoRepository(ISqlConnectionFactory connectionFactory) : IIndicadorVisaoRepository
{
    private const string ColunaDia = "CONVERT(char(10), E.DATAHORAENT, 23)";
    private const string FormatoDia = "yyyy-MM-dd";
    private const string FormatoRotulo = "dd/MM";
    private const string CorUnica = "#0d6efd";

    private const string CondicaoFechamento = "E.FECHADO <> 'C'";
    private const string CondicaoLote = "E.LoteEnt <> 'INAT'";
    private const string CondicaoPeriodoInicio = "E.DATAHORAENT >= @DataInicio";
    private const string CondicaoPeriodoFim = "E.DATAHORAENT < @DataFim";
    private const string CondicaoTipoExame = "E.TIPO = @Tipo";

    private const string IndicadorId = "exames";
    private const string IndicadorNome = "Exames";
    private const string IndicadorUnidade = "exames";

    private const string TipoExame = "3";

    private static readonly IReadOnlyList<DimensaoVisao> DimensoesDisponiveis = [DimensaoVisao.Convenio];

    private static readonly IReadOnlyList<(CoverageCategory Cobertura, string Nome, string Cor)> SeriesConvenio =
    [
        (CoverageCategory.Particular, "Particular", "#0d6efd"),
        (CoverageCategory.Convenio, "Convênio", "#198754"),
        (CoverageCategory.Sus, "SUS", "#dc3545"),
    ];

    private static readonly IReadOnlyDictionary<CoverageCategory, string> ModoPorCobertura =
        new Dictionary<CoverageCategory, string>
        {
            [CoverageCategory.Particular] = "P",
            [CoverageCategory.Convenio] = "C",
            [CoverageCategory.Sus] = "S",
        };

    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IndicadorVisao> ObterVisaoAsync(
        IndicatorFilter filter,
        DimensaoVisao dimensao,
        GraficoForma forma,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!DimensoesDisponiveis.Contains(dimensao))
        {
            dimensao = DimensaoVisao.Nenhuma;
        }

        if (dimensao == DimensaoVisao.Nenhuma && forma == GraficoForma.Donut)
        {
            forma = GraficoForma.Coluna;
        }

        BusinessReferencePeriod periodo = PeriodoResolutor.Resolver(filter);
        DateTime inicio = periodo.Start.ToDateTime(TimeOnly.MinValue);
        DateTime fim = periodo.End.AddDays(1).ToDateTime(TimeOnly.MinValue);

        using IDbConnection connection = _connectionFactory.Create();

        IReadOnlyDictionary<DateOnly, int> totalPorDia = await ConsultarPorDia(
            connection,
            AtendimentosRepository.ConstruirClausulaFrom(false),
            [],
            inicio,
            fim,
            null,
            cancellationToken);

        IReadOnlyList<(string Nome, string Cor, IReadOnlyDictionary<DateOnly, int> PorDia)> series = dimensao == DimensaoVisao.Convenio
            ? await ConsultarSeriesConvenio(connection, inicio, fim, cancellationToken)
            : [(Nome: IndicadorNome, Cor: CorUnica, PorDia: totalPorDia)];

        return MontarVisao(periodo, totalPorDia.Values.Sum(), dimensao, forma, series);
    }

    private async Task<IReadOnlyList<(string Nome, string Cor, IReadOnlyDictionary<DateOnly, int> PorDia)>> ConsultarSeriesConvenio(
        IDbConnection connection,
        DateTime inicio,
        DateTime fim,
        CancellationToken cancellationToken)
    {
        var series = new List<(string Nome, string Cor, IReadOnlyDictionary<DateOnly, int> PorDia)>(SeriesConvenio.Count);

        foreach ((CoverageCategory cobertura, string nome, string cor) in SeriesConvenio)
        {
            IReadOnlyDictionary<DateOnly, int> porDia = await ConsultarPorDia(
                connection,
                AtendimentosRepository.ConstruirClausulaFrom(true),
                AtendimentosRepository.ConstruirCondicoesCobertura(cobertura),
                inicio,
                fim,
                ModoPorCobertura[cobertura],
                cancellationToken);

            series.Add((Nome: nome, Cor: cor, PorDia: porDia));
        }

        return series;
    }

    private async Task<IReadOnlyDictionary<DateOnly, int>> ConsultarPorDia(
        IDbConnection connection,
        string clausulaFrom,
        IReadOnlyList<string> condicoesAdicionais,
        DateTime inicio,
        DateTime fim,
        string? modoFatura,
        CancellationToken cancellationToken)
    {
        var parametros = new DynamicParameters();
        parametros.Add("DataInicio", inicio);
        parametros.Add("DataFim", fim);
        parametros.Add("Tipo", TipoExame);

        if (modoFatura is not null)
        {
            parametros.Add("ModoFatura", modoFatura);
        }

        var comando = new CommandDefinition(
            ConstruirSqlDiario(clausulaFrom, condicoesAdicionais),
            parametros,
            cancellationToken: cancellationToken);

        LinhaDiaria[] linhas = (await connection.QueryAsync<LinhaDiaria>(comando)).ToArray();

        var porDia = new Dictionary<DateOnly, int>(linhas.Length);
        foreach (LinhaDiaria linha in linhas)
        {
            porDia[DateOnly.ParseExact(linha.Dia, FormatoDia, CultureInfo.InvariantCulture)] = linha.Quantidade;
        }

        return porDia;
    }

    internal static string ConstruirSqlDiario(string clausulaFrom, IReadOnlyList<string> condicoesAdicionais)
    {
        var condicoes = new List<string>(5 + condicoesAdicionais.Count)
        {
            CondicaoPeriodoInicio,
            CondicaoPeriodoFim,
            CondicaoFechamento,
            CondicaoLote,
            CondicaoTipoExame,
        };
        condicoes.AddRange(condicoesAdicionais);

        return $"SELECT {ColunaDia} AS Dia, COUNT(1) AS Quantidade "
             + $"FROM {clausulaFrom} "
             + $"WHERE {string.Join(" AND ", condicoes)} "
             + $"GROUP BY {ColunaDia} "
             + "ORDER BY Dia";
    }

    internal static IndicadorVisao MontarVisao(
        BusinessReferencePeriod periodo,
        decimal valorTotal,
        DimensaoVisao dimensao,
        GraficoForma forma,
        IReadOnlyList<(string Nome, string Cor, IReadOnlyDictionary<DateOnly, int> PorDia)> series)
    {
        List<DateOnly> dias = EnumerarDias(periodo).ToList();

        return new IndicadorVisao
        {
            Id = IndicadorId,
            Nome = IndicadorNome,
            Unidade = IndicadorUnidade,
            Monetario = false,
            Periodo = periodo,
            ValorTotal = valorTotal,
            Forma = forma,
            DimensaoAtiva = dimensao,
            DimensoesDisponiveis = [.. DimensoesDisponiveis],
            Series =
            [
                .. series.Select(s => new SerieGrafico
                {
                    Nome = s.Nome,
                    Cor = s.Cor,
                    Pontos =
                    [
                        .. dias.Select(dia => new PontoGrafico
                        {
                            Data = dia,
                            Rotulo = dia.ToString(FormatoRotulo, CultureInfo.InvariantCulture),
                            Valor = s.PorDia.GetValueOrDefault(dia),
                        }),
                    ],
                }),
            ],
        };
    }

    internal static IEnumerable<DateOnly> EnumerarDias(BusinessReferencePeriod periodo)
    {
        for (int offset = 0; offset <= periodo.End.DayNumber - periodo.Start.DayNumber; offset++)
        {
            yield return periodo.Start.AddDays(offset);
        }
    }

    private sealed class LinhaDiaria
    {
        public string Dia { get; set; } = string.Empty;

        public int Quantidade { get; set; }
    }
}