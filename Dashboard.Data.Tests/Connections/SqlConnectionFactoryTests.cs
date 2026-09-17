using System.Data;
using Dapper;
using Dashboard.Data.Connections;

namespace Dashboard.Data.Tests.Connections;

public sealed class SqlConnectionFactoryTests
{
    [Fact]
    public async Task factory_abre_conexao_com_banco_sisac_html5()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SisacDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Fail(
                "Lacuna de credencial: variável de ambiente 'ConnectionStrings__SisacDatabase' não configurada. " +
                "O teste de integração depende da credencial de leitura do banco CASAMATER (dashboard_readonly). " +
                "Configure a variável no ambiente de execução e execute novamente.");
        }

        ISqlConnectionFactory factory = new SqlConnectionFactory(connectionString);
        using IDbConnection connection = factory.Create();

        var result = await connection.ExecuteScalarAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }
}