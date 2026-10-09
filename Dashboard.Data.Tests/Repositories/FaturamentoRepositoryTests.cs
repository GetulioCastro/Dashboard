using System.Reflection;
using Dashboard.Core.DTOs;
using Dashboard.Data.Connections;
using Dashboard.Data.Repositories;
using Microsoft.Data.SqlClient;

namespace Dashboard.Data.Tests.Repositories;

public sealed class FaturamentoRepositoryTests
{
    private const string VariavelConexao = "ConnectionStrings__SisacDatabase";

    [Fact]
    public async Task CA_04_faturamento_realizado_periodo_com_dados_retorna_valor_positivo()
    {
        var repository = new FaturamentoRepository(new SqlConnectionFactory(ObterConnectionString()));

        FaturamentoDto resultado = await ExecutarAsync(
            repository,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));

        Assert.True(resultado.ValorFaturado > 0m, "Janeiro/2026 deve ter faturamento realizado (F+E).");
        Assert.Equal(new DateOnly(2026, 1, 1), resultado.DataInicial);
        Assert.Equal(new DateOnly(2026, 1, 31), resultado.DataFinal);
    }

    [Fact]
    public async Task CA_04_faturamento_realizado_periodo_sem_dados_retorna_zero()
    {
        var repository = new FaturamentoRepository(new SqlConnectionFactory(ObterConnectionString()));

        FaturamentoDto resultado = await ExecutarAsync(
            repository,
            new DateOnly(1990, 1, 1),
            new DateOnly(1990, 1, 31));

        Assert.Equal(0m, resultado.ValorFaturado);
    }

    [Fact]
    public void CA_04_sql_considera_somente_os_estados_f_e_e()
    {
        Assert.Contains("E.Fechado IN ('F','E')", FaturamentoRepository.Sql, StringComparison.Ordinal);
        Assert.DoesNotContain("'A'", FaturamentoRepository.Sql, StringComparison.Ordinal);
        Assert.DoesNotContain("'C'", FaturamentoRepository.Sql, StringComparison.Ordinal);
        Assert.DoesNotContain("'P'", FaturamentoRepository.Sql, StringComparison.Ordinal);
        Assert.DoesNotContain("'X'", FaturamentoRepository.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void CA_04_sql_usa_parametros_de_periodo_com_fim_exclusivo()
    {
        Assert.Contains("E.DataHoraEnt >= @DataInicial", FaturamentoRepository.Sql, StringComparison.Ordinal);
        Assert.Contains("E.DataHoraEnt < @DataFinalExclusiva", FaturamentoRepository.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void CA_04_sql_e_constante_sem_datas_literais_ou_concatenacao()
    {
        FieldInfo campo = typeof(FaturamentoRepository)
            .GetField("Sql", BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.True(campo.IsLiteral, "O SQL deve ser uma constante de compilação (sem concatenação em runtime).");
        Assert.DoesNotContain("2024", FaturamentoRepository.Sql, StringComparison.Ordinal);
        Assert.DoesNotContain("2026", FaturamentoRepository.Sql, StringComparison.Ordinal);
    }

    private static async Task<FaturamentoDto> ExecutarAsync(
        FaturamentoRepository repository,
        DateOnly dataInicial,
        DateOnly dataFinal)
    {
        try
        {
            return await repository.ObterFaturamentoRealizadoAsync(dataInicial, dataFinal);
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
                $"Lacuna de credencial: a variavel de ambiente '{VariavelConexao}' nao esta configurada. "
                + "O teste de integracao T-11 depende da credencial de leitura do banco CASAMATER (dashboard_readonly).");
        }

        return connectionString;
    }
}
