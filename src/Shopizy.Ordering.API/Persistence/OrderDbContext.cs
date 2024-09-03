using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shopizy.Domain.Models.Base;
using Shopizy.Domain.Models.Persistence;
using Shopizy.Ordering.API.Aggregates;

namespace Shopizy.Ordering.API.Persistence;

public class OrderDbContext(DbContextOptions options, IHttpContextAccessor _httpContextAccessor, IPublisher _publisher) : DbContext(options), IAppDbContext
{
    public DbSet<Order> Orders { get; set; }
    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get the domain events from the entity framework change tracker
        var domainEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .SelectMany(entry => entry.Entity.PopDomainEvents())
            .ToList();

        if (IsUserWaitingOnline())
        {
            AddDomainEventsToOfflineProcessingQueue(domainEvents);
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Publish all the domain event
        await PublishDomainEvents(domainEvents);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<List<IDomainEvent>>().ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private bool IsUserWaitingOnline() => _httpContextAccessor.HttpContext is not null;

    private async Task PublishDomainEvents(List<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent);
        }
    }

    private void AddDomainEventsToOfflineProcessingQueue(List<IDomainEvent> domainEvents)
    {
        // Get pending domain events from session
        Queue<IDomainEvent> domainEventsQueue = _httpContextAccessor.HttpContext!.Items.TryGetValue(EventualConsistencyMiddleware.DomainEventsKey, out object? value) &&
            value is Queue<IDomainEvent> existingDomainEvents
            ? existingDomainEvents : new();

        // Add new domain event to the Queue
        domainEvents.ForEach(domainEventsQueue.Enqueue);

        // Update the session with newly added events
        _httpContextAccessor.HttpContext.Items[EventualConsistencyMiddleware.DomainEventsKey] = domainEventsQueue;
    }
}
