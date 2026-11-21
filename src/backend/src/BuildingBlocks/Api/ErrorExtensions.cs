using Laundry.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Http;

namespace Laundry.BuildingBlocks.Api;

public static class ErrorExtensions
{
    public static IResult ToProblem(this Error error) =>
        Results.Problem(
            type: error.Code,
            detail: error.Description,
            statusCode: StatusFor(error.Type));

    private static int StatusFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };
}
