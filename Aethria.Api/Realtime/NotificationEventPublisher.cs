using Aethria.Api.Hubs;
using Aethria.Application.UseCases.Notifications;
using Aethria.Application.UseCases.Notifications.GetPageNotifications;
using Microsoft.AspNetCore.SignalR;

namespace Aethria.Api.Realtime;

internal sealed class NotificationEventPublisher : INotificationEventPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationEventPublisher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishCreatedAsync(
        Guid userId,
        NotificationPageItemResponse notification,
        CancellationToken cancellationToken)
    {
        return _hubContext.Clients
            .User(userId.ToString())
            .SendAsync("NotificationCreated", notification, cancellationToken);
    }
}
