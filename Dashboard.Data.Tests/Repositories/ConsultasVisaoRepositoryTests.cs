using Dashboard.Core.DTOs;
using Dashboard.Core.Regras;
using Dashboard.Data.Connections;
using Dashboard.Data.Repositories;
using Microsoft.Data.SqlClient;

namespace Dashboard.Data.Tests.Repositories;

public class ConsultasVisaoRepositoryTests
{
    private const string VariavelConexao = "ConnectionStrings__SisacDatabase";

    private static IndicatorFilter FiltroPassado(DateOnly inicio, DateOnly fim) => new()
    {
        Period = PeriodType.Past,
        StartDate = inicio,
        EndDate = fim,
    };

    [Fact]
    public void sql_diario_filtra_tipo_consulta_e_preserva_rn07()
    {
        string sql = ConsultasVisaoRepository.ConstruirSqlDiario(
            AtendimentosRepository.ConstruirClausulaFrom(false),
            []);

        Assert.Contains("FROM dbo.ENTRADA E", sql, StringComparison.Ordinal);
        Assert.Contains("E.TIPO = @Tipo", sql, StringComparison.Ordinal);
        Assert.Contains("E.DATAHORAENT >= @DataInicio", sql, StringComparison.Ordinal);
        Assert.Contains("E.DATAHORAENT < @DataFim", sql, StringComparison.Ordinal);
        Assert.Contains("E.FECHADO <> 'C'", sql, StringComparison.Ordinal);
        Assert.Contains("E.LoteEnt <> 'INAT'", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("CADCONVENIO", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void sql_diario_nao_inclui_retorno_pois_retorno_fora_do_indicador_consultas()
    {
        string sql = ConsultasVisaoRepository.ConstruirSqlDiario(
            AtendimentosRepository.ConstruirClausulaFrom(false),
            []);

        Assert.DoesNotContain("IN ('1', '2')", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("'2'", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void sql_diario_dimensao_convenio_usa_join_contextual_e_exclusao_de_suspenso()
    {
        string sql = ConsultasVisaoRepository.ConstruirSqlDiario(
            AtendimentosRepository.ConstruirClausulaFrom(true),
            AtendimentosRepository.ConstruirCondicoesCobertura(CoverageCategory.Convenio));

        Assert.Contains("INNER JOIN dbo.CADCONVENIO C ON C.CODCONVENIO = E.CODCONVENIO", sql, StringComparison.Ordinal);
        Assert.Contains("C.GRUPOEMP = E.GRUPOEMP", sql, StringComparison.Ordinal);
        Assert.Contains("C.FILIAL = E.FILIAL", sql, StringComparison.Ordinal);
        Assert.Contains("C.MODOFAT = @ModoFatura", sql, StringComparison.Ordinal);
        Assert.Contains("COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'", sql, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(CoverageCategory.Particular)]
    [InlineData(CoverageCategory.Sus)]
    public void sql_diario_categorias_nao_suspenso_nao_aplicam_a_exclusao_de_suspenso(CoverageCategory cobertura)
    {
        string sql = ConsultasVisaoRepository.ConstruirSqlDiario(
            AtendimentosRepository.ConstruirClausulaFrom(true),
            AtendimentosRepository.ConstruirCondicoesCobertura(cobertura));

        Assert.Contains("C.MODOFAT = @ModoFatura", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("SUSPENSO", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void enumerar_dias_inclui_inicio_e_fim()
    {
        var periodo = new BusinessReferencePeriod(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 5));

        DateOnly[] dias = ConsultasVisaoRepository.EnumerarDias(periodo).ToArray();

        Assert.Equal(5, dias.Length);
        Assert.Equal(new DateOnly(2026, 3, 1), dias[0]);
        Assert.Equal(new DateOnly(2026, 3, 5), dias[^1]);
    }

    [Fact]
    public void montar_visao_identifica_consultas_e_preenche_dias_sem_consulta_com_zero()
    {
        var periodo = new BusinessReferencePeriod(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 3));
        var comDados = new Dictionary<DateOnly, int> { [new DateOnly(2026, 3, 2)] = 4 };
        var series = new List<(string, string, IReadOnlyDictionary<DateOnly, int>)>
        {
            ("Particular", "#0d6efd", comDados),
        };

        IndicadorVisao visao = ConsultasVisaoRepository.MontarVisao(
            periodo,
            4m,
            DimensaoVisao.Convenio,
            GraficoForma.Coluna,
            series);

        Assert.Equal("consultas", visao.Id);
        Assert.Equal("Consultas", visao.Nome);
        Assert.Equal("consultas", visao.Unidade);
        Assert.Equal(4m, visao.ValorTotal);
        Assert.Equal(DimensaoVisao.Convenio, visao.DimensaoAtiva);
        Assert.False(visao.Monetario);
        Assert.Equal(periodo, visao.Periodo);
        Assert.DoesNotContain(DimensaoVisao.Sexo, visao.DimensoesDisponiveis);
        Assert.Contains(DimensaoVisao.Convenio, visao.DimensoesDisponiveis);

        PontoGrafico[] pontos = Assert.Single(visao.Series).Pontos.ToArray();
        Assert.Equal(3, pontos.Length);
        Assert.Equal(0m, pontos[0].Valor);
        Assert.Equal(4m, pontos[1].Valor);
        Assert.Equal(0m, pontos[2].Valor);
    }

    [Fact]
    public async Task CA_11_consultas_no_periodo_retorna_quantidade()
    {
        IndicadorVisao visao = await ConsultarBancoAsync(
            FiltroPassado(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)),
            DimensaoVisao.Nenhuma,
            GraficoForma.Coluna);

        Assert.Equal("consultas", visao.Id);
        Assert.Equal(31, Assert.Single(visao.Series).Pontos.Count);
        Assert.Equal(visao.Series[0].Pontos.Sum(p => p.Valor), visao.ValorTotal);
        Assert.Equal(31, visao.Periodo.End.DayNumber - visao.Periodo.Start.DayNumber + 1);
    }

    [Fact]
    public async Task CA_11_consultas_nao_excede_atendimentos_no_mesmo_periodo()
    {
        IndicadorVisao consultas = await ConsultarBancoAsync(
            FiltroPassado(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)),
            DimensaoVisao.Nenhuma,
            GraficoForma.Coluna);

        IndicadorVisao atendimentos = await ConsultarBancoComAtendimentosAsync(
            FiltroPassado(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)),
            DimensaoVisao.Nenhuma,
            GraficoForma.Coluna);

        Assert.Equal("atendimentos", atendimentos.Id);
        Assert.True(
            consultas.ValorTotal <= atendimentos.ValorTotal,
            $"Consultas ({consultas.ValorTotal}) nao pode exceder Atendimentos ({atendimentos.ValorTotal}) no mesmo periodo.");
    }

    [Fact]
    public async Task CA_18_consultas_por_cobertura_gera_tres_series_sem_exceder_o_total()
    {
        IndicadorVisao visao = await ConsultarBancoAsync(
            FiltroPassado(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)),
            DimensaoVisao.Convenio,
            GraficoForma.Coluna);

        Assert.Equal(3, visao.Series.Count);
        Assert.Equal("Particular", visao.Series[0].Nome);
        Assert.Equal("Convênio", visao.Series[1].Nome);
        Assert.Equal("SUS", visao.Series[2].Nome);
        Assert.All(visao.Series, s => Assert.Equal(31, s.Pontos.Count));
        Assert.True(visao.Series.Sum(s => s.Pontos.Sum(p => p.Valor)) <= visao.ValorTotal);
    }

    [Fact]
    public async Task CA_09_consultas_periodo_sem_dados_retorna_serie_com_zeros_e_total_zero()
    {
        IndicadorVisao visao = await ConsultarBancoAsync(
            FiltroPassado(new DateOnly(1990, 1, 1), new DateOnly(1990, 1, 7)),
            DimensaoVisao.Nenhuma,
            GraficoForma.Coluna);

        Assert.Equal(7, Assert.Single(visao.Series).Pontos.Count);
        Assert.Equal(0m, visao.ValorTotal);
        Assert.All(visao.Series[0].Pontos, p => Assert.Equal(0m, p.Valor));
    }

    private static async Task<IndicadorVisao> ConsultarBancoAsync(
        IndicatorFilter filter,
        DimensaoVisao dimensao,
        GraficoForma forma)
    {
        var repository = new ConsultasVisaoRepository(new SqlConnectionFactory(ObterConnectionString()));

        try
        {
            return await repository.ObterVisaoAsync(filter, dimensao, forma);
        }
        catch (SqlException erro)
        {
            Assert.Fail(
                "Teste de integracao BLOQUEADO por permissao de leitura no banco. "
                + $"A credencial de leitura nao possui SELECT necessario. Detalhe: {erro.Message}");
            throw;
        }
    }

    private static async Task<IndicadorVisao> ConsultarBancoComAtendimentosAsync(
        IndicatorFilter filter,
        DimensaoVisao dimensao,
        GraficoForma forma)
    {
        var repository = new AtendimentosVisaoRepository(new SqlConnectionFactory(ObterConnectionString()));

        try
        {
            return await repository.ObterVisaoAsync(filter, dimensao, forma);
        }
        catch (SqlException erro)
        {
            Assert.Fail(
                "Teste de integracao BLOQUEADO por permissao de leitura no banco. "
                + $"A credencial de leitura nao possui SELECT necessario. Detalhe: {erro.Message}");
            throw;
        }
    }

    private static string ObterConnectionString()
    {
        string? connectionString = Environment.GetEnvironmentVariable(VariavelConexao);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail(
                $"Variavel de ambiente {VariavelConexao} nao configurada. "
                + "O teste de integracao exige a credencial de leitura do Dashboard, fornecida por fora do repositorio.");
        }

        return connectionString;
    }
}