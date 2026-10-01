using System.Data;
using Dapper;
using Dashboard.Core.Contratos;
using Dashboard.Core.DTOs;
using Dashboard.Data.Connections;
using Dashboard.Data.Repositories;

namespace Dashboard.Data.Tests.Repositories;

public sealed class AtendimentosRepositoryTests
{
    private const string VariavelCredencial = "ConnectionStrings__SisacDatabase";

    private static readonly DateOnly Inicio2024 = new(2024, 1, 1);
    private static readonly DateOnly Fim2024 = new(2024, 12, 31);
    private static readonly DateTime DataHoraInicio2024 = new(2024, 1, 1);
    private static readonly DateTime DataHoraFimExclusivo2025 = new(2025, 1, 1);

    [Fact]
    public async Task CA_03_atendimentos_por_periodo_retorna_quantidade_correta()
    {
        string? connectionString = CredencialDeLeitura();
        IIndicatorRepository repository = new AtendimentosRepository(new SqlConnectionFactory(connectionString));

        var filter = new IndicatorFilter
        {
            Period = PeriodType.Past,
            StartDate = Inicio2024,
            EndDate = Fim2024,
        };

        IndicatorData resultado = await repository.GetIndicatorAsync(filter);

        long esperado = await ContarAtendimentosIndependente(connectionString!, DataHoraInicio2024, DataHoraFimExclusivo2025);

        Assert.NotEqual(0m, resultado.Value);
        Assert.Equal((decimal)esperado, resultado.Value);
        Assert.Equal(Inicio2024, resultado.ReferencePeriod.Start);
        Assert.Equal(Fim2024, resultado.ReferencePeriod.End);
    }

    [Fact]
    public void CA_18_join_cobertura_usa_chave_contextual_codconvenio_grupoemp_filial()
    {
        const string esperado =
            "dbo.ENTRADA E INNER JOIN dbo.CADCONVENIO C ON C.CODCONVENIO = E.CODCONVENIO AND C.GRUPOEMP = E.GRUPOEMP AND C.FILIAL = E.FILIAL";

        Assert.Equal(esperado, AtendimentosRepository.ConstruirClausulaFrom(comCobertura: true));
        Assert.Equal("dbo.ENTRADA E", AtendimentosRepository.ConstruirClausulaFrom(comCobertura: false));
    }

    [Fact]
    public void CA_18_cobertura_convenio_exclui_convenios_suspensos()
    {
        IReadOnlyList<string> condicoes = AtendimentosRepository.ConstruirCondicoesCobertura(CoverageCategory.Convenio);

        Assert.Contains("C.MODOFAT = @ModoFatura", condicoes);
        Assert.Contains("COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'", condicoes);
        Assert.Equal(2, condicoes.Count);
    }

    [Fact]
    public void CA_18_cobertura_particular_e_sus_nao_excluem_por_suspensao()
    {
        Assert.Equal(
            new[] { "C.MODOFAT = @ModoFatura" },
            AtendimentosRepository.ConstruirCondicoesCobertura(CoverageCategory.Particular));

        Assert.Equal(
            new[] { "C.MODOFAT = @ModoFatura" },
            AtendimentosRepository.ConstruirCondicoesCobertura(CoverageCategory.Sus));
    }

    [Fact]
    public async Task CA_18_atendimentos_por_cobertura_particular_convenio_sus()
    {
        string? connectionString = CredencialDeLeitura();
        IIndicatorRepository repository = new AtendimentosRepository(new SqlConnectionFactory(connectionString));

        IndicatorData total = await repository.GetIndicatorAsync(new IndicatorFilter
        {
            Period = PeriodType.Past,
            StartDate = Inicio2024,
            EndDate = Fim2024,
        });

        Assert.True(total.Value > 0, "Ano de 2024 deve ter atendimentos registrados no banco CASAMATER.");

        decimal somaCategorias = 0;
        foreach (CoverageCategory cobertura in new[] { CoverageCategory.Particular, CoverageCategory.Convenio, CoverageCategory.Sus })
        {
            IndicatorData categoria = await repository.GetIndicatorAsync(new IndicatorFilter
            {
                Period = PeriodType.Past,
                StartDate = Inicio2024,
                EndDate = Fim2024,
                Coverage = cobertura,
            });

            long esperado = await ContarAtendimentosIndependente(
                connectionString!, DataHoraInicio2024, DataHoraFimExclusivo2025, ModoFatura(cobertura));

            Assert.Equal((decimal)esperado, categoria.Value);
            somaCategorias += categoria.Value;
        }

        Assert.True(
            somaCategorias <= total.Value,
            "A soma das categorias Particular + Convênio + SUS não pode exceder o total (MODOFAT vazio/NULL ⇒ 'Não classificado').");
    }

    [Fact]
    public async Task atendimentos_periodo_sem_dados_retorna_vazio_ca09()
    {
        string? connectionString = CredencialDeLeitura();
        IIndicatorRepository repository = new AtendimentosRepository(new SqlConnectionFactory(connectionString));

        var filter = new IndicatorFilter
        {
            Period = PeriodType.Future,
            StartDate = new DateOnly(2040, 1, 1),
            EndDate = new DateOnly(2040, 12, 31),
        };

        IndicatorData resultado = await repository.GetIndicatorAsync(filter);

        Assert.Equal(0m, resultado.Value);
        Assert.Equal(filter.StartDate.Value, resultado.ReferencePeriod.Start);
        Assert.Equal(filter.EndDate.Value, resultado.ReferencePeriod.End);
    }

    private static string? CredencialDeLeitura()
    {
        string? connectionString = Environment.GetEnvironmentVariable(VariavelCredencial);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail(
                $"Lacuna de credencial: a variável de ambiente '{VariavelCredencial}' não está configurada. " +
                "O teste de integração T-08 depende da credencial de leitura do banco CASAMATER (dashboard_readonly) " +
                "para validar os valores reais. Configure a variável no ambiente de execução e execute novamente.");
        }

        return connectionString;
    }

    private static async Task<long> ContarAtendimentosIndependente(
        string connectionString, DateTime dataInicio, DateTime dataFim, string? modoFatura = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.ENTRADA E
            LEFT JOIN dbo.CADCONVENIO C
              ON C.CODCONVENIO = E.CODCONVENIO
             AND C.GRUPOEMP = E.GRUPOEMP
             AND C.FILIAL = E.FILIAL
            WHERE E.DATAHORAENT >= @DataInicio
              AND E.DATAHORAENT < @DataFim
              AND E.FECHADO <> 'C'
              AND E.LoteEnt <> 'INAT'
            """;

        string sqlFinal = sql;
        if (modoFatura is not null)
        {
            sqlFinal += " AND C.MODOFAT = @ModoFatura";
            if (modoFatura == ModoFatura(CoverageCategory.Convenio))
            {
                sqlFinal += " AND COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'";
            }
        }

        using IDbConnection connection = new SqlConnectionFactory(connectionString).Create();

        var parametros = new DynamicParameters();
        parametros.Add("DataInicio", dataInicio);
        parametros.Add("DataFim", dataFim);
        if (modoFatura is not null)
        {
            parametros.Add("ModoFatura", modoFatura);
        }

        return await connection.ExecuteScalarAsync<long>(sqlFinal, parametros);
    }

    private static string ModoFatura(CoverageCategory cobertura) => cobertura switch
    {
        CoverageCategory.Particular => "P",
        CoverageCategory.Convenio => "C",
        CoverageCategory.Sus => "S",
        _ => throw new ArgumentOutOfRangeException(nameof(cobertura)),
    };
}