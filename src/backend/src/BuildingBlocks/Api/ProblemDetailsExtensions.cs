using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Laundry.BuildingBlocks.Api;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;
                if (problem.Type is null || problem.Type.StartsWith("https://", StringComparison.Ordinal))
                {
                    problem.Type = SlugFor(problem.Status ?? context.HttpContext.Response.StatusCode);
                }
            });

        return services;
    }

    public static IApplicationBuilder UseApiProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }

    private static string SlugFor(int status) => status switch
    {
        400 => "bad-request",
        401 => "unauthorized",
        403 => "forbidden",
        404 => "not-found",
        405 => "method-not-allowed",
        409 => "conflict",
        500 => "internal-error",
        _ => $"http-{status}",
    };
}
