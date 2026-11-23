namespace Laundry.Host.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddApiDocs(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Laundry Platform API";
                document.Info.Version = "v1";
                document.Info.Description = "HTTP API of the laundry platform: laundries, machines, bookings and faults.";
                return Task.CompletedTask;
            }));

        return services;
    }

    public static WebApplication UseApiDocs(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Laundry Platform API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
