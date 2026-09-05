using Microsoft.EntityFrameworkCore;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.Enums;

namespace Shopizy.CatalogService.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Slug == normalizedSlug, ct);
    }

    public async Task<PagedResult<ProductListResponse>> SearchAsync(ProductQueryParameters parameters, CancellationToken ct = default)
    {
        var query = _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => p.Status != ProductStatus.Archived);

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.BrandId.HasValue)
        {
            query = query.Where(p => p.BrandId == parameters.BrandId.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice.Amount >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice.Amount <= parameters.MaxPrice.Value);
        }

        if (parameters.InStockOnly == true)
        {
            query = query.Where(p => p.Variants.Any(v => v.StockQuantity > 0));
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        query = parameters.SortBy?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.BasePrice.Amount),
            "price_desc" => query.OrderByDescending(p => p.BasePrice.Amount),
            "name_asc" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };

        var totalCount = await query.CountAsync(ct);

        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Fetch category & brand names
        var categoryIds = items.Select(i => i.CategoryId).Distinct().ToList();
        var brandIds = items.Select(i => i.BrandId).Distinct().ToList();

        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var brands = await _context.Brands
            .Where(b => brandIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        var dtos = items.Select(p =>
        {
            var mainImage = p.Images.FirstOrDefault(i => i.IsMain)?.Url ?? p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url;
            var totalStock = p.Variants.Sum(v => v.StockQuantity);
            var isInStock = totalStock > 0;
            categories.TryGetValue(p.CategoryId, out var catName);
            brands.TryGetValue(p.BrandId, out var brandName);

            return new ProductListResponse(
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Status.ToString(),
                p.BasePrice.Amount,
                p.BasePrice.Currency,
                p.Version,
                catName,
                brandName,
                mainImage,
                totalStock,
                isInStock);
        }).ToList();

        return PagedResult<ProductListResponse>.Create(dtos, totalCount, page, pageSize);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await _context.Products
            .AnyAsync(p => p.Slug == normalizedSlug && (!excludeId.HasValue || p.Id != excludeId.Value), ct);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default)
    {
        var normalizedSku = sku.Trim().ToUpperInvariant();
        return await _context.ProductVariants
            .AnyAsync(v => v.Sku == normalizedSku, ct);
    }
}
