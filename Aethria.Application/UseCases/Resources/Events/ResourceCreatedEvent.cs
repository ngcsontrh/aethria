using DispatchR.Abstractions.Notification;

namespace Aethria.Application.UseCases.Resources.Events;

public sealed record ResourceCreatedEvent(
    Guid ResourceId,
    Guid UserId) : INotification;
