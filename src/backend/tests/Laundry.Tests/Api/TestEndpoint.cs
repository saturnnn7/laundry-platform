using Laundry.BuildingBlocks.Api;
using Laundry.BuildingBlocks.Domain;

namespace Laundry.Tests.Api;

public sealed class TestEndpoint : IEndpoint
{
    public const string ExceptionMessage = "secret-failure-message";

    public void Map(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/test/ok", () => Results.Ok(new { value = 1 }));
        routes.MapGet("/test/not-found", () => Error.NotFound("test-missing", "The thing is missing.").ToProblem());
        routes.MapGet("/test/conflict", () => Error.Conflict("test-conflict", "The thing clashes.").ToProblem());
        routes.MapGet("/test/throw", IResult () => throw new InvalidOperationException(ExceptionMessage));
    }
}

public abstract class AbstractTestEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) => routes.MapGet("/test/abstract", () => Results.Ok());
}
