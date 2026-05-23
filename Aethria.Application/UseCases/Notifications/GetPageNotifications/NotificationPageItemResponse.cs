namespace Aethria.Application.UseCases.Notifications.GetPageNotifications;

public sealed record NotificationPageItemResponse(
    Guid Id,
    string Type,
    IReadOnlyDictionary<string, string> Data,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
