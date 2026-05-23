using Aethria.Api.Endpoints.Notifications;
using Aethria.Application.UseCases.Notifications.GetPageNotifications;
using Aethria.Application.UseCases.Notifications.MarkNotificationAsRead;
using Aethria.Application.UseCases.Notifications.MarkNotificationsAsRead;

namespace Aethria.Api.Endpoints;

internal static class NotificationEndpoints
{
    internal static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/notifications")
            .RequireAuthorization()
            .WithTags("Notifications");

        group.MapGet("", GetPage)
            .WithName("GetNotifications")
            .Produces<PagedResponse<NotificationPageItemResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPatch("read", MarkManyAsRead)
            .WithName("MarkNotificationsAsRead")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPatch("{id:guid}/read", MarkAsRead)
            .WithName("MarkNotificationAsRead")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Get a paginated list of notifications for the authenticated user.
    /// </summary>
    public static async Task<IResult> GetPage(
        [AsParameters] GetPageNotificationsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetPageNotificationsQuery(
            user.GetUserId(),
            request.PageNumber,
            request.PageSize,
            request.IsRead);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Mark notifications as read.
    /// </summary>
    public static async Task<IResult> MarkManyAsRead(
        [FromBody] MarkNotificationsAsReadRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new MarkNotificationsAsReadCommand(
            UserId: user.GetUserId(),
            NotificationIds: request.NotificationIds);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    public static async Task<IResult> MarkAsRead(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new MarkNotificationAsReadCommand(
            UserId: user.GetUserId(),
            NotificationId: id);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }
}
