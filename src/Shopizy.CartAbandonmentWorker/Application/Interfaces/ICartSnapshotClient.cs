using Shopizy.CartAbandonmentWorker.Application.Contracts;

namespace Shopizy.CartAbandonmentWorker.Application.Interfaces;

public interface ICartSnapshotClient
{
    Task<List<CartSnapshotDto>> GetActiveCartsAsync();
    void RegisterCartSnapshot(CartSnapshotDto cart);
}
