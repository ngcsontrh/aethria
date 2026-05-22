namespace Aethria.Domain.Events;

public sealed record ChatSessionCreatedEvent(Guid SessionId, Guid UserId) : DomainEvent;

public sealed record ChatMessageSentEvent(
    Guid SessionId,
    Guid MessageId,
    string Role) : DomainEvent;
