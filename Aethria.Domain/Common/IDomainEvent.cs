using DispatchR.Abstractions.Notification;

namespace Aethria.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
