using Aethria.Api.Endpoints.Mentors;
using Aethria.Application.UseCases.Mentors.CreateMentor;
using Aethria.Application.UseCases.Mentors.DeleteMentor;
using Aethria.Application.UseCases.Mentors.GetMentorById;
using Aethria.Application.UseCases.Mentors.GetPageMentors;
using Aethria.Application.UseCases.Mentors.UpdateMentor;

namespace Aethria.Api.Endpoints;

internal static class MentorEndpoints
{
    internal static void MapMentorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/mentors")
            .RequireAuthorization()
            .WithTags("Mentors");

        group.MapGet("", GetPage)
            .WithName("GetMentors")
            .Produces<PagedResponse<MentorPageItemResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("{id:guid}", GetById)
            .WithName("GetMentorById")
            .Produces<GetMentorByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", Create)
            .WithName("CreateMentor")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .Produces(StatusCodes.Status504GatewayTimeout);

        group.MapPut("{id:guid}", Update)
            .WithName("UpdateMentor")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .Produces(StatusCodes.Status504GatewayTimeout);

        group.MapDelete("{id:guid}", Delete)
            .WithName("DeleteMentor")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get a paginated list of mentors for the current user.
    /// </summary>
    public static async Task<IResult> GetPage(
        [AsParameters] GetPageMentorsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetPageMentorsQuery(
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
    /// Get a mentor by its unique identifier.
    /// </summary>
    public static async Task<IResult> GetById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetMentorByIdQuery(
            MentorId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Create a new mentor.
    /// </summary>
    public static async Task<IResult> Create(
        [FromBody] CreateMentorRequest model,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new CreateMentorCommand(
            Name: model.Name,
            Description: model.Description,
            Instruction: model.Instruction,
            Tools: model.Tools,
            UserId: user.GetUserId());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.CreatedAtRoute("GetMentorById", new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Update an existing mentor.
    /// </summary>
    public static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMentorRequest model,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMentorCommand(
            MentorId: id,
            Name: model.Name,
            Description: model.Description,
            Instruction: model.Instruction,
            Tools: model.Tools,
            UserId: user.GetUserId());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok();
    }

    /// <summary>
    /// Delete a mentor and all associated chat sessions and messages.
    /// </summary>
    public static async Task<IResult> Delete(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMentorCommand(
            MentorId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok();
    }
}
