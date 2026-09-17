using System.Data;

namespace Dashboard.Data.Connections;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}