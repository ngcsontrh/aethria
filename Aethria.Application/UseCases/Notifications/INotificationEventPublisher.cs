using Aethria.Application.UseCases.Notifications.GetPageNotifications;

namespace Aethria.Application.UseCases.Notifications;

public interface INotificationEventPublisher
{
    Task PublishCreatedAsync(
        Guid userId,
        NotificationPageItemResponse notification,
        CancellationToken cancellationToken);
}
