using Shopizy.CartAbandonmentWorker.Application.Contracts;
using Shopizy.CartAbandonmentWorker.Application.Interfaces;

namespace Shopizy.CartAbandonmentWorker.Infrastructure.Clients;

public class MockCartSnapshotClient : ICartSnapshotClient
{
    private readonly List<CartSnapshotDto> _snapshots = new();

    public void RegisterCartSnapshot(CartSnapshotDto cart)
    {
        _snapshots.RemoveAll(c => c.CartId == cart.CartId);
        _snapshots.Add(cart);
    }

    public Task<List<CartSnapshotDto>> GetActiveCartsAsync()
    {
        return Task.FromResult(_snapshots.ToList());
    }
}
