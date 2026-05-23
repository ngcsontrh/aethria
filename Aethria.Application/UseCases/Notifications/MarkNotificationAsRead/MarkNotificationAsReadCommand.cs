namespace Aethria.Application.UseCases.Notifications.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid UserId,
    Guid NotificationId) : IRequest<MarkNotificationAsReadCommand, ValueTask<Result>>;

