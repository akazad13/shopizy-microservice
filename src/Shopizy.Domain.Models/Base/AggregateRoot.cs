namespace Shopizy.Domain.Models.Base;

public abstract class AggregateRoot<TId, TIdType> : Entity<TId> where TId : AggregateRootId<TIdType>
{
    protected AggregateRoot(TId id)
    {
        Id = id;
    }

    protected AggregateRoot() { }
}

