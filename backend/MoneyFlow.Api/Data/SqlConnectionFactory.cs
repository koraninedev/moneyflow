using System.Data;
using Microsoft.Data.SqlClient;

namespace MoneyFlow.Api.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("MoneyFlowDb")
        ?? throw new InvalidOperationException("Connection string 'MoneyFlowDb' is not configured.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
