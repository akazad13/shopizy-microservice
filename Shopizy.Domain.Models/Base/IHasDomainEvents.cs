namespace Shopizy.Domain.Models.Models;
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    List<IDomainEvent> PopDomainEvents();
}
