namespace Aethria.Domain.Events;

public sealed record ResourceCreatedEvent(Guid ResourceId, Guid UserId) : DomainEvent;
