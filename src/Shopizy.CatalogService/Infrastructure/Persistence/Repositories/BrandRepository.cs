using Microsoft.EntityFrameworkCore;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.CatalogService.Domain.Entities;

namespace Shopizy.CatalogService.Infrastructure.Persistence.Repositories;

public sealed class BrandRepository : IBrandRepository
{
    private readonly CatalogDbContext _context;

    public BrandRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await _context.Brands.FirstOrDefaultAsync(b => b.Slug == normalizedSlug, ct);
    }

    public async Task<IReadOnlyCollection<Brand>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var query = _context.Brands.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(b => b.IsActive);
        }

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(Brand brand, CancellationToken ct = default)
    {
        await _context.Brands.AddAsync(brand, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Brand brand, CancellationToken ct = default)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Brands.AnyAsync(b => b.Id == id, ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await _context.Brands
            .AnyAsync(b => b.Slug == normalizedSlug && (!excludeId.HasValue || b.Id != excludeId.Value), ct);
    }
}
