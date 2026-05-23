namespace Aethria.Application.UseCases.Notifications.GetPageNotifications;

public sealed record GetPageNotificationsQuery(
    Guid UserId,
    int PageNumber,
    int PageSize,
    bool? IsRead) : IRequest<GetPageNotificationsQuery, ValueTask<Result<PagedResponse<NotificationPageItemResponse>>>>;

