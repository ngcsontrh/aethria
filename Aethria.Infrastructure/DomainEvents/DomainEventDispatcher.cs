using DispatchR;

namespace Aethria.Infrastructure.DomainEvents;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly AppDbContext _dbContext;
    private readonly IMediator _mediator;

    public DomainEventDispatcher(AppDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task DispatchEventsAsync(CancellationToken cancellationToken)
    {
        var aggregatesWithEvents = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await PublishConcreteAsync((dynamic)domainEvent, cancellationToken);
        }
    }

    private async ValueTask PublishConcreteAsync<TDomainEvent>(
        TDomainEvent domainEvent,
        CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        await _mediator.Publish(domainEvent, cancellationToken);
    }
}
