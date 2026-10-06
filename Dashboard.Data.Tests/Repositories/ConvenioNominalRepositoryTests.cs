using Dashboard.Core.DTOs;
using Dashboard.Core.Regras;
using Dashboard.Data.Connections;
using Dashboard.Data.Repositories;
using Microsoft.Data.SqlClient;

namespace Dashboard.Data.Tests.Repositories;

public class ConvenioNominalRepositoryTests
{
    private const string VariavelConexao = "ConnectionStrings__SisacDatabase";

    private static readonly DateOnly PeriodoInteiroInicio = new(1900, 1, 1);
    private static readonly DateOnly PeriodoInteiroFim = new(2100, 1, 1);

    private static readonly DateOnly AnoComDadosInicio = new(2024, 1, 1);
    private static readonly DateOnly AnoComDadosFim = new(2024, 12, 31);

    private static IndicatorFilter FiltroPassado(DateOnly inicio, DateOnly fim) => new()
    {
        Period = PeriodType.Past,
        StartDate = inicio,
        EndDate = fim,
    };

    private static string SqlDe(string indicador) => indicador switch
    {
        "atendimentos" => AtendimentosConvenioNominalRepository.Sql,
        "consultas" => ConsultasConvenioNominalRepository.Sql,
        "exames" => ExamesConvenioNominalRepository.Sql,
        _ => throw new ArgumentOutOfRangeException(nameof(indicador), indicador, "Indicador nominal desconhecido."),
    };

    [Theory]
    [InlineData("atendimentos")]
    [InlineData("consultas")]
    [InlineData("exames")]
    public void sql_nominal_resolve_identificacao_pela_chave_composta(string indicador)
    {
        string sql = SqlDe(indicador);

        Assert.Contains(
            "INNER JOIN dbo.CADCONVENIO C ON C.CODCONVENIO = E.CODCONVENIO AND C.GRUPOEMP = E.GRUPOEMP AND C.FILIAL = E.FILIAL",
            sql,
            StringComparison.Ordinal);
        Assert.Contains("C.DESCR AS Identificacao", sql, StringComparison.Ordinal);
        Assert.Contains("GROUP BY C.CODCONVENIO, C.GRUPOEMP, C.FILIAL, C.DESCR", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("LOWER(", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("atendimentos")]
    [InlineData("consultas")]
    [InlineData("exames")]
    public void sql_nominal_preserva_o_conjunto_valido_do_indicador_rn07(string indicador)
    {
        string sql = SqlDe(indicador);

        Assert.Contains("E.DATAHORAENT >= @DataInicio", sql, StringComparison.Ordinal);
        Assert.Contains("E.DATAHORAENT < @DataFim", sql, StringComparison.Ordinal);
        Assert.Contains("E.FECHADO <> 'C'", sql, StringComparison.Ordinal);
        Assert.Contains("COALESCE(E.LoteEnt,'') <> 'INAT'", sql, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("atendimentos")]
    [InlineData("consultas")]
    [InlineData("exames")]
    public void sql_nominal_preserva_p19_sem_introduzir_regra_de_modofat(string indicador)
    {
        string sql = SqlDe(indicador);

        Assert.Contains("COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("MODOFAT", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("atendimentos")]
    [InlineData("consultas")]
    [InlineData("exames")]
    public void sql_nominal_ordena_volume_decrescente_com_desempate_az_e_chave_composta(string indicador)
    {
        string sql = SqlDe(indicador);

        Assert.Contains(
            "ORDER BY COUNT(1) DESC, C.DESCR ASC, C.CODCONVENIO ASC, C.GRUPOEMP ASC, C.FILIAL ASC",
            sql,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("atendimentos")]
    [InlineData("consultas")]
    [InlineData("exames")]
    public void sql_nominal_e_select_somente_leitura_sem_paginacao_nem_top_n(string indicador)
    {
        string sql = SqlDe(indicador);

        Assert.StartsWith("SELECT", sql.TrimStart(), StringComparison.OrdinalIgnoreCase);

        foreach (string proibido in new[]
                 {
                     "INSERT", "UPDATE", "DELETE", "MERGE", "CREATE", "ALTER", "DROP",
                     "TRUNCATE", "EXEC", "INTO", "TOP", "OFFSET", "FETCH",
                 })
        {
            Assert.DoesNotContain(proibido, sql, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void sql_nominal_filtra_tipo_somente_em_consultas_e_exames()
    {
        Assert.Contains("E.TIPO = @Tipo", ConsultasConvenioNominalRepository.Sql, StringComparison.Ordinal);
        Assert.Contains("E.TIPO = @Tipo", ExamesConvenioNominalRepository.Sql, StringComparison.Ordinal);
        Assert.DoesNotContain("E.TIPO", AtendimentosConvenioNominalRepository.Sql, StringComparison.Ordinal);
    }

    private static (DateOnly Inicio, DateOnly Fim) PeriodoPara(string indicador) =>
        indicador == "atendimentos"
            ? (AnoComDadosInicio, AnoComDadosFim)
            : (PeriodoInteiroInicio, PeriodoInteiroFim);

    [Theory]
    [InlineData("atendimentos")]
    public async Task CA_003_01_listagem_de_atendimentos_com_identificacao_e_volume(string indicador)
    {
        (DateOnly inicio, DateOnly fim) = PeriodoPara(indicador);

        IndicadorConvenioNominalVisao visao = await ConsultarAsync(indicador, inicio, fim);

        Assert.NotEmpty(visao.Convenios);
        Assert.All(visao.Convenios, c => Assert.False(string.IsNullOrWhiteSpace(c.Identificacao)));
        Assert.All(visao.Convenios, c => Assert.True(c.Volume > 0));
        Assert.Equal(visao.Convenios.Sum(c => c.Volume), visao.ValorTotal);
        Assert.Equal(inicio, visao.Periodo.Start);
        Assert.Equal(fim, visao.Periodo.End);
    }

    [Theory]
    [InlineData("consultas")]
    [InlineData("exames")]
    public async Task CA_003_03_listagem_de_consultas_e_exames_com_identificacao_e_volume(string indicador)
    {
        IndicadorConvenioNominalVisao visao = await ConsultarAsync(
            indicador,
            PeriodoInteiroInicio,
            PeriodoInteiroFim);

        Assert.NotEmpty(visao.Convenios);
        Assert.All(visao.Convenios, c => Assert.False(string.IsNullOrWhiteSpace(c.Identificacao)));
        Assert.All(visao.Convenios, c => Assert.True(c.Volume > 0));
        Assert.Equal(visao.Convenios.Sum(c => c.Volume), visao.ValorTotal);
    }

    [Theory]
    [InlineData("atendimentos")]
    [InlineData("consultas")]
    [InlineData("exames")]
    public async Task CA_003_04_ordenacao_por_volume_decrescente_na_listagem(string indicador)
    {
        (DateOnly inicio, DateOnly fim) = PeriodoPara(indicador);

        IndicadorConvenioNominalVisao visao = await ConsultarAsync(indicador, inicio, fim);

        Assert.NotEmpty(visao.Convenios);
        Assert.True(visao.Convenios.Count > 1, "A listagem deve ter mais de um convenio para validar a ordenacao.");

        for (var i = 1; i < visao.Convenios.Count; i++)
        {
            Assert.True(
                visao.Convenios[i - 1].Volume >= visao.Convenios[i].Volume,
                $"Volume fora de ordem decrescente na posicao {i}: "
                + $"{visao.Convenios[i - 1].Volume} antes de {visao.Convenios[i].Volume}.");
        }

        IndicadorConvenioNominalVisao recarga = await ConsultarAsync(indicador, inicio, fim);

        Assert.Equal(
            visao.Convenios.Select(c => (c.Identificacao, c.Volume)),
            recarga.Convenios.Select(c => (c.Identificacao, c.Volume)));
    }

    [Fact]
    public async Task CA_003_07_estado_vazio_periodo_sem_dados_retorna_lista_vazia()
    {
        IndicadorConvenioNominalVisao visao = await ConsultarAsync(
            "atendimentos",
            new DateOnly(1990, 1, 1),
            new DateOnly(1990, 1, 7));

        Assert.Empty(visao.Convenios);
        Assert.Equal(0m, visao.ValorTotal);
    }

    [Fact]
    public async Task estado_de_erro_propaga_falha_para_a_camada_superior()
    {
        var repository = new AtendimentosConvenioNominalRepository(new SqlConnectionFactory(null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ObterConvenioNominalAsync(
            FiltroPassado(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31))));
    }

    private static async Task<IndicadorConvenioNominalVisao> ConsultarAsync(
        string indicador,
        DateOnly inicio,
        DateOnly fim)
    {
        string? connectionString = Environment.GetEnvironmentVariable(VariavelConexao);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail(
                $"Variavel de ambiente {VariavelConexao} nao configurada. "
                + "O teste de integracao exige a credencial de leitura do Dashboard, fornecida por fora do repositorio.");
        }

        IndicatorFilter filtro = FiltroPassado(inicio, fim);
        var connectionFactory = new SqlConnectionFactory(connectionString);

        try
        {
            return indicador switch
            {
                "atendimentos" => await new AtendimentosConvenioNominalRepository(connectionFactory)
                    .ObterConvenioNominalAsync(filtro),
                "consultas" => await new ConsultasConvenioNominalRepository(connectionFactory)
                    .ObterConvenioNominalAsync(filtro),
                "exames" => await new ExamesConvenioNominalRepository(connectionFactory)
                    .ObterConvenioNominalAsync(filtro),
                _ => throw new ArgumentOutOfRangeException(nameof(indicador), indicador, null),
            };
        }
        catch (SqlException erro)
        {
            Assert.Fail(
                erro.Number == -2
                    ? $"Consulta nominal excedeu o tempo limite de 30s (erro {erro.Number}). "
                      + "Reduza a janela do periodo do teste. Detalhe: " + erro.Message
                    : "Teste de integracao BLOQUEADO por permissao de leitura no banco. "
                      + $"A credencial de leitura nao possui SELECT necessario (erro {erro.Number}). "
                      + $"Detalhe: {erro.Message}");
            throw;
        }
    }
}
