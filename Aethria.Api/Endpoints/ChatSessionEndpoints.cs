using Aethria.Api.Endpoints.ChatSessions;
using Aethria.Application.UseCases.ChatSessions;
using Aethria.Application.UseCases.ChatSessions.DeleteAllChatSessions;
using Aethria.Application.UseCases.ChatSessions.DeleteAllUserChatSessions;
using Aethria.Application.UseCases.ChatSessions.DeleteChatSession;
using Aethria.Application.UseCases.ChatSessions.GetChatHistory;
using Aethria.Application.UseCases.ChatSessions.GetChatSessions;
using ApiGetChatHistoryResponse = Aethria.Api.Endpoints.ChatSessions.GetChatHistoryResponse;
using ApiGetChatMessageItemResponse = Aethria.Api.Endpoints.ChatSessions.GetChatMessageItemResponse;
using ApiGetChatSessionItemResponse = Aethria.Api.Endpoints.ChatSessions.GetChatSessionItemResponse;

namespace Aethria.Api.Endpoints;

internal static class ChatSessionEndpoints
{
    internal static void MapChatSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/chat-sessions")
            .RequireAuthorization()
            .WithTags("ChatSessions");

        group.MapGet("general", GetGeneralChatSessions)
            .WithName("GetGeneralChatSessions")
            .Produces<PagedResponse<ApiGetChatSessionItemResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("general/{sessionId:guid}/messages", GetGeneralChatHistory)
            .WithName("GetGeneralChatHistory")
            .Produces<ApiGetChatHistoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("general/{sessionId:guid}", DeleteGeneralChatSession)
            .WithName("DeleteGeneralChatSession")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("general", DeleteAllGeneralChatSessions)
            .WithName("DeleteAllGeneralChatSessions")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("mentors/{mentorId:guid}", GetMentorChatSessions)
            .WithName("GetMentorChatSessions")
            .Produces<PagedResponse<ApiGetChatSessionItemResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("mentors/{mentorId:guid}/{sessionId:guid}/messages", GetMentorChatHistory)
            .WithName("GetMentorChatHistory")
            .Produces<ApiGetChatHistoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("mentors/{mentorId:guid}/{sessionId:guid}", DeleteMentorChatSession)
            .WithName("DeleteMentorChatSession")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("mentors/{mentorId:guid}", DeleteAllMentorChatSessions)
            .WithName("DeleteAllMentorChatSessions")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("resources/{resourceId:guid}", GetResourceChatSessions)
            .WithName("GetResourceChatSessions")
            .Produces<PagedResponse<ApiGetChatSessionItemResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("resources/{resourceId:guid}/{sessionId:guid}/messages", GetResourceChatHistory)
            .WithName("GetResourceChatHistory")
            .Produces<ApiGetChatHistoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("resources/{resourceId:guid}/{sessionId:guid}", DeleteResourceChatSession)
            .WithName("DeleteResourceChatSession")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("resources/{resourceId:guid}", DeleteAllResourceChatSessions)
            .WithName("DeleteAllResourceChatSessions")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("", DeleteAllUserChatSessions)
            .WithName("DeleteAllUserChatSessions")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Get paginated general chat sessions for the current user.
    /// </summary>
    public static Task<IResult> GetGeneralChatSessions(
        [AsParameters] GetChatSessionsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return GetChatSessions(request, ChatSessionScope.General, null, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Get all chat history messages for a general chat session.
    /// </summary>
    public static Task<IResult> GetGeneralChatHistory(
        [FromRoute] Guid sessionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return GetChatHistory(sessionId, ChatSessionScope.General, null, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete a specific general chat session and all its messages.
    /// </summary>
    public static Task<IResult> DeleteGeneralChatSession(
        [FromRoute] Guid sessionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return DeleteChatSession(sessionId, ChatSessionScope.General, null, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete all general chat sessions for the current user.
    /// </summary>
    public static Task<IResult> DeleteAllGeneralChatSessions(
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return DeleteAllChatSessions(ChatSessionScope.General, null, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Get paginated chat sessions for a specific mentor.
    /// </summary>
    public static Task<IResult> GetMentorChatSessions(
        [FromRoute] Guid mentorId,
        [AsParameters] GetChatSessionsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return GetChatSessions(request, ChatSessionScope.Mentor, mentorId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Get chat history messages for a specific mentor chat session.
    /// </summary>
    public static Task<IResult> GetMentorChatHistory(
        [FromRoute] Guid mentorId,
        [FromRoute] Guid sessionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return GetChatHistory(sessionId, ChatSessionScope.Mentor, mentorId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete a specific mentor chat session and all its messages.
    /// </summary>
    public static Task<IResult> DeleteMentorChatSession(
        [FromRoute] Guid mentorId,
        [FromRoute] Guid sessionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return DeleteChatSession(sessionId, ChatSessionScope.Mentor, mentorId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete all chat sessions for a specific mentor.
    /// </summary>
    public static Task<IResult> DeleteAllMentorChatSessions(
        [FromRoute] Guid mentorId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return DeleteAllChatSessions(ChatSessionScope.Mentor, mentorId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Get paginated chat sessions for a specific resource.
    /// </summary>
    public static Task<IResult> GetResourceChatSessions(
        [FromRoute] Guid resourceId,
        [AsParameters] GetChatSessionsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return GetChatSessions(request, ChatSessionScope.Resource, resourceId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Get chat history messages for a specific resource chat session.
    /// </summary>
    public static Task<IResult> GetResourceChatHistory(
        [FromRoute] Guid resourceId,
        [FromRoute] Guid sessionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return GetChatHistory(sessionId, ChatSessionScope.Resource, resourceId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete a specific resource chat session and all its messages.
    /// </summary>
    public static Task<IResult> DeleteResourceChatSession(
        [FromRoute] Guid resourceId,
        [FromRoute] Guid sessionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return DeleteChatSession(sessionId, ChatSessionScope.Resource, resourceId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete all chat sessions for a specific resource.
    /// </summary>
    public static Task<IResult> DeleteAllResourceChatSessions(
        [FromRoute] Guid resourceId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return DeleteAllChatSessions(ChatSessionScope.Resource, resourceId, mediator, user, cancellationToken);
    }

    /// <summary>
    /// Delete all chat sessions for the current user.
    /// </summary>
    public static async Task<IResult> DeleteAllUserChatSessions(
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAllUserChatSessionsCommand(user.GetUserId());
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetChatSessions(
        GetChatSessionsRequest request,
        ChatSessionScope scope,
        Guid? scopeId,
        IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetChatSessionsQuery(
            UserId: user.GetUserId(),
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            Scope: scope,
            ScopeId: scopeId);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        var items = result.Value.Items.Select(s => new ApiGetChatSessionItemResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();

        var response = new PagedResponse<ApiGetChatSessionItemResponse>(
            items,
            result.Value.TotalCount,
            result.Value.PageNumber,
            result.Value.PageSize
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> GetChatHistory(
        Guid sessionId,
        ChatSessionScope scope,
        Guid? scopeId,
        IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetChatHistoryQuery(
            SessionId: sessionId,
            UserId: user.GetUserId(),
            Scope: scope,
            ScopeId: scopeId);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        var response = new ApiGetChatHistoryResponse
        {
            Messages = [.. result.Value.Messages.Select(m => new ApiGetChatMessageItemResponse
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })]
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteChatSession(
        Guid sessionId,
        ChatSessionScope scope,
        Guid? scopeId,
        IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteChatSessionCommand(
            SessionId: sessionId,
            UserId: user.GetUserId(),
            Scope: scope,
            ScopeId: scopeId);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAllChatSessions(
        ChatSessionScope scope,
        Guid? scopeId,
        IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAllChatSessionsCommand(
            UserId: user.GetUserId(),
            Scope: scope,
            ScopeId: scopeId);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }
}
