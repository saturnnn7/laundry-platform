using Microsoft.AspNetCore.Routing;

namespace Laundry.BuildingBlocks.Api;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder routes);
}
