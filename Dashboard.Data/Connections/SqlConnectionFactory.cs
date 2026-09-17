using System.Data;
using Microsoft.Data.SqlClient;

namespace Dashboard.Data.Connections;

public sealed class SqlConnectionFactory(string? connectionString) : ISqlConnectionFactory
{
    private readonly string? _connectionString = connectionString;

    public IDbConnection Create()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:SisacDatabase' não configurada. " +
                "Configure a credencial de leitura via User Secrets (dev) ou variável de ambiente.");
        }

        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}