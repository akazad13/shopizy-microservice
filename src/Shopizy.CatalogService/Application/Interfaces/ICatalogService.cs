using Shopizy.CatalogService.Application.Contracts;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Application.Interfaces;

public interface ICatalogService
{
    // Categories
    Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<CategoryResponse>>> GetCategoriesAsync(bool activeOnly = true, CancellationToken ct = default);

    // Brands
    Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandRequest request, CancellationToken ct = default);
    Task<Result<BrandResponse>> UpdateBrandAsync(Guid id, UpdateBrandRequest request, CancellationToken ct = default);
    Task<Result<BrandResponse>> GetBrandByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<BrandResponse>>> GetBrandsAsync(bool activeOnly = true, CancellationToken ct = default);

    // Products
    Task<Result<ProductDetailResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<Result<ProductDetailResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<Result<ProductDetailResponse>> GetProductByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<PagedResult<ProductListResponse>>> SearchProductsAsync(ProductQueryParameters parameters, CancellationToken ct = default);
    Task<Result<ProductVariantResponse>> AddVariantAsync(Guid productId, ProductVariantDto request, CancellationToken ct = default);
    Task<Result<ProductVariantResponse>> UpdateVariantStockAsync(Guid productId, Guid variantId, StockAdjustmentRequest request, CancellationToken ct = default);
    Task<Result<bool>> ArchiveProductAsync(Guid id, CancellationToken ct = default);
}
