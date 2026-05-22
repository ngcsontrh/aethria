using Aethria.Api.Endpoints.ApiKeys;
using Aethria.Application.UseCases.ApiKeys.CreateApiKey;
using Aethria.Application.UseCases.ApiKeys.GetPageApiKeys;
using Aethria.Application.UseCases.ApiKeys.RevokeApiKey;

namespace Aethria.Api.Endpoints;

internal static class ApiKeyEndpoints
{
    internal static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/api-keys")
            .RequireAuthorization()
            .WithTags("ApiKeys");

        group.MapPost("", Create)
            .WithName("CreateApiKey")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("", GetPage)
            .WithName("GetApiKeys")
            .Produces<PagedResponse<ApiKeyPageItemResponse>>(StatusCodes.Status200OK);

        group.MapDelete("{id:guid}", Revoke)
            .WithName("RevokeApiKey")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Create a new API key.
    /// </summary>
    public static async Task<IResult> Create(
        [FromBody] CreateApiKeyRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        var command = new CreateApiKeyCommand(
            UserId: userId,
            Email: email,
            Name: request.Name,
            ExpirationDays: request.ExpirationDays);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Json(result.Value, statusCode: StatusCodes.Status201Created);
    }

    /// <summary>
    /// Get a paginated list of API keys for the authenticated user.
    /// </summary>
    public static async Task<IResult> GetPage(
        [AsParameters] GetPageApiKeysRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var query = new GetPageApiKeysQuery(userId, request.PageNumber, request.PageSize);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Revoke an API key.
    /// </summary>
    public static async Task<IResult> Revoke(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        var command = new RevokeApiKeyCommand(
            UserId: userId,
            KeyId: id);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }
}
