using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Laundry.Host.Health;

public sealed class PostgresHealthCheck : IHealthCheck
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    private readonly IServiceProvider _services;

    public PostgresHealthCheck(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        try
        {
            var dataSource = _services.GetRequiredService<NpgsqlDataSource>();
            await using var connection = await dataSource.OpenConnectionAsync(timeout.Token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(timeout.Token);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL check failed.", exception);
        }
    }
}
