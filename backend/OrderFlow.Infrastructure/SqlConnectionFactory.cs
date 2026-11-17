using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace OrderFlow.Infrastructure;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderFlow");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The OrderFlow connection string is missing or empty.");
        }

        _connectionString = connectionString;
    }
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
