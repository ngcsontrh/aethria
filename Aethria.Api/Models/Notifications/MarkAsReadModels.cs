namespace Aethria.Api.Endpoints.Notifications;

/// <summary>
/// Request body for marking notifications as read.
/// </summary>
public sealed record MarkNotificationsAsReadRequest
{
    /// <summary>
    /// Notification IDs to mark as read.
    /// </summary>
    public IReadOnlyCollection<Guid> NotificationIds { get; init; } = [];
}

