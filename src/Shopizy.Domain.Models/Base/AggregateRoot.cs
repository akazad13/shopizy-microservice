namespace Shopizy.Domain.Models.Models;

public abstract class AggregateRoot<TId, TIdType> : Entity<TId> where TId : AggregateRootId<TIdType>
{
    protected AggregateRoot(TId id)
    {
        Id = id;
    }

    protected AggregateRoot() { }
}

