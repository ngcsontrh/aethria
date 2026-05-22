namespace Aethria.Infrastructure.DomainEvents;

internal interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(CancellationToken cancellationToken);
}
