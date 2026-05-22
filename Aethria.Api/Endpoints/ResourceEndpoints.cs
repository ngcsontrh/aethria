using Aethria.Api.Endpoints.Resources;
using Aethria.Application.UseCases.Resources.CreateResource;
using Aethria.Application.UseCases.Resources.DeleteResource;
using Aethria.Application.UseCases.Resources.DownloadResource;
using Aethria.Application.UseCases.Resources.GetPageResources;
using Aethria.Application.UseCases.Resources.GetResourceById;
using Aethria.Application.UseCases.Resources.GetResourceSelector;
using Aethria.Application.UseCases.Resources.UpdateResource;

namespace Aethria.Api.Endpoints;

internal static class ResourceEndpoints
{
    internal static void MapResourceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/resources")
            .RequireAuthorization()
            .WithTags("Resources");

        group.MapPost("", Create)
            .DisableAntiforgery()
            .WithName("CreateResource")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}", Update)
            .WithName("UpdateResource")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}", GetById)
            .WithName("GetResourceById")
            .Produces<GetResourceByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/download", Download)
            .WithName("DownloadResource")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("", GetPage)
            .WithName("GetResources")
            .Produces<PagedResponse<ResourcePageItemResponse>>(StatusCodes.Status200OK);

        group.MapGet("selector", GetSelector)
            .WithName("GetResourceSelector")
            .Produces<GetResourceSelectorResponse>(StatusCodes.Status200OK);

        group.MapDelete("{id:guid}", Delete)
            .WithName("DeleteResource")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Upload and create a new learning resource.
    /// </summary>
    public static async Task<IResult> Create(
        [FromForm] CreateResourceRequest model,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        using var fileStream = model.File.OpenReadStream();
        var command = new CreateResourceCommand(
            Name: model.Name,
            Description: model.Description,
            FileStream: fileStream,
            FileName: model.File.FileName,
            ContentType: model.File.ContentType,
            FileSize: model.File.Length,
            UserId: user.GetUserId()
        );

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.CreatedAtRoute("GetResourceById", new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Update a learning resource.
    /// </summary>
    public static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateResourceRequest model,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new UpdateResourceCommand(
            ResourceId: id,
            Name: model.Name,
            Description: model.Description,
            UserId: user.GetUserId()
        );

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok();
    }

    /// <summary>
    /// Get a learning resource by ID.
    /// </summary>
    public static async Task<IResult> GetById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetResourceByIdQuery(id, user.GetUserId());
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Download a learning resource file by ID.
    /// </summary>
    public static async Task<IResult> Download(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new DownloadResourceQuery(id, user.GetUserId());
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>
    /// Get a paginated list of learning resources.
    /// </summary>
    public static async Task<IResult> GetPage(
        [AsParameters] GetPageResourcesRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetPageResourcesQuery(user.GetUserId(), request.PageNumber, request.PageSize);
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Get all learning resources for lightweight selector controls.
    /// </summary>
    public static async Task<IResult> GetSelector(
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetResourceSelectorQuery(user.GetUserId());
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Delete a learning resource.
    /// </summary>
    public static async Task<IResult> Delete(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteResourceCommand(id, user.GetUserId());
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok();
    }
}
