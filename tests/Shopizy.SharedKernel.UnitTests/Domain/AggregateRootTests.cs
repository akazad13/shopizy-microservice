using FluentAssertions;
using Shopizy.SharedKernel.Domain;
using Xunit;

namespace Shopizy.SharedKernel.UnitTests.Domain;

public class AggregateRootTests
{
    private sealed record ItemCreatedDomainEvent(Guid ItemId) : IDomainEvent;

    private sealed class OrderAggregate : AggregateRoot<Guid>
    {
        public OrderAggregate(Guid id) : base(id) { }

        public void CompleteCreation()
        {
            RaiseDomainEvent(new ItemCreatedDomainEvent(Id));
        }
    }

    [Fact]
    public void RaiseDomainEvent_EnqueuesEventInCollection()
    {
        var id = Guid.NewGuid();
        var aggregate = new OrderAggregate(id);

        aggregate.CompleteCreation();

        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.First().Should().BeOfType<ItemCreatedDomainEvent>()
            .Which.ItemId.Should().Be(id);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesEventCollection()
    {
        var aggregate = new OrderAggregate(Guid.NewGuid());
        aggregate.CompleteCreation();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }
}
