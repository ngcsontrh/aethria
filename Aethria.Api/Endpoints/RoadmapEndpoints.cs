using Aethria.Api.Endpoints.Roadmaps;
using Aethria.Application.UseCases.Roadmaps.DeleteRoadmap;
using Aethria.Application.UseCases.Roadmaps.GetPageRoadmaps;
using Aethria.Application.UseCases.Roadmaps.GetRoadmapById;

namespace Aethria.Api.Endpoints;

internal static class RoadmapEndpoints
{
    internal static void MapRoadmapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/roadmaps")
            .RequireAuthorization()
            .WithTags("Roadmaps");

        group.MapGet("", GetRoadmaps)
            .WithName("GetRoadmaps")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("{id:guid}", GetRoadmapById)
            .WithName("GetRoadmapById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", DeleteRoadmap)
            .WithName("DeleteRoadmap")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get paginated roadmaps.
    /// </summary>
    public static async Task<IResult> GetRoadmaps(
        [AsParameters] GetPageRoadmapsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetPageRoadmapsQuery(
            UserId: user.GetUserId(),
            PageNumber: request.PageNumber,
            PageSize: request.PageSize);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Get a roadmap by ID.
    /// </summary>
    public static async Task<IResult> GetRoadmapById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetRoadmapByIdQuery(
            RoadmapId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Delete a roadmap by ID.
    /// </summary>
    public static async Task<IResult> DeleteRoadmap(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRoadmapCommand(
            RoadmapId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }
}
