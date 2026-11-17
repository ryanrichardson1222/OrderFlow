using Microsoft.Data.SqlClient;

namespace OrderFlow.Infrastructure;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
