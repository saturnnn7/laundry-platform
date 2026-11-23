using Npgsql;

namespace Laundry.Host.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("Postgres");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("The connection string 'ConnectionStrings:Postgres' is not configured.");
            }

            return NpgsqlDataSource.Create(connectionString);
        });

        return services;
    }
}
