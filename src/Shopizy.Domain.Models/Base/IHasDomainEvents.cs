namespace Shopizy.Domain.Models.Base;
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    List<IDomainEvent> PopDomainEvents();
}
