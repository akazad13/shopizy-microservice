namespace Shopizy.SharedKernel.Domain;

/// <summary>
/// Marker interface for in-process domain events raised by Aggregate Roots.
/// </summary>
public interface IDomainEvent
{
    Guid EventId => Guid.NewGuid();
    DateTime OccurredAtUtc => DateTime.UtcNow;
}
