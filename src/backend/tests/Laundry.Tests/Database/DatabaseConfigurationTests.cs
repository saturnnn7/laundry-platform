using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace Laundry.Tests.Database;

public class DatabaseConfigurationTests
{
    private static WebApplicationFactory<Program> CreateFactory(string connectionString) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Postgres", connectionString);
        });

    [Fact]
    public void DataSource_IsCreatedFromConfiguration_WithoutConnecting()
    {
        const string connectionString = "Host=localhost;Port=5432;Database=laundry;Username=laundry";
        using var factory = CreateFactory(connectionString);

        var dataSource = factory.Services.GetRequiredService<NpgsqlDataSource>();
        var builder = new NpgsqlConnectionStringBuilder(dataSource.ConnectionString);

        Assert.Equal("localhost", builder.Host);
        Assert.Equal("laundry", builder.Database);
    }

    [Fact]
    public void DataSource_WithEmptyConnectionString_ThrowsAndNamesTheKey()
    {
        using var factory = CreateFactory(string.Empty);

        var exception = Assert.Throws<InvalidOperationException>(
            () => factory.Services.GetRequiredService<NpgsqlDataSource>());

        Assert.Contains("ConnectionStrings:Postgres", exception.Message);
    }
}
