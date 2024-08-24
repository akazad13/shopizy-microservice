namespace Shopizy.Domain.Models.Persistence;

public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
