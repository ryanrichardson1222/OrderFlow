using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace OrderFlow.Infrastructure.Tests;

public class SqlConnectionFactoryTests
{
    [Fact]
    public void Constructor_WithMissingConnectionString_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => new SqlConnectionFactory(configuration));

        Assert.Equal(
            "The OrderFlow connection string is missing or empty.",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyConnectionString_ThrowsInvalidOperationException(
        string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:OrderFlow"] = connectionString
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => new SqlConnectionFactory(configuration));

        Assert.Equal(
            "The OrderFlow connection string is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void CreateConnection_WithValidConnectionString_ReturnsSqlConnection()
    {
        const string expectedConnectionString =
            "Server=localhost;Database=OrderFlowTest;Integrated Security=True;TrustServerCertificate=True";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:OrderFlow"] = expectedConnectionString
            })
            .Build();

        var factory = new SqlConnectionFactory(configuration);

        using var connection = factory.CreateConnection();

        Assert.IsType<SqlConnection>(connection);
        Assert.Equal(expectedConnectionString, connection.ConnectionString);
    }
}
