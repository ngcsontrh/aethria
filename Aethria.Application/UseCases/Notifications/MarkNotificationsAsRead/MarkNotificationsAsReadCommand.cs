namespace Aethria.Application.UseCases.Notifications.MarkNotificationsAsRead;

public sealed record MarkNotificationsAsReadCommand(
    Guid UserId,
    IReadOnlyCollection<Guid> NotificationIds) : IRequest<MarkNotificationsAsReadCommand, ValueTask<Result>>;

