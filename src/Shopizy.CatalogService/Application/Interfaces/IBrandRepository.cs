using Shopizy.CatalogService.Domain.Entities;

namespace Shopizy.CatalogService.Application.Interfaces;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyCollection<Brand>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task AddAsync(Brand brand, CancellationToken ct = default);
    Task UpdateAsync(Brand brand, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
}
