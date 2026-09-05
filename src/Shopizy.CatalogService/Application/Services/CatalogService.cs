using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Application.Services;

public sealed class CatalogService : ICatalogService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductRepository _productRepository;

    public CatalogService(
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _productRepository = productRepository;
    }

    #region Categories

    public async Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(request.ParentCategoryId.Value, ct);
            if (!parentExists)
            {
                return Result.Failure<CategoryResponse>(Error.NotFound(
                    "Category.ParentNotFound",
                    $"Parent category with ID '{request.ParentCategoryId.Value}' does not exist."));
            }
        }

        var slugExists = await _categoryRepository.SlugExistsAsync(request.Slug, null, ct);
        if (slugExists)
        {
            return Result.Failure<CategoryResponse>(Error.Conflict(
                "Category.DuplicateSlug",
                $"A category with slug '{request.Slug}' already exists."));
        }

        var categoryResult = Category.Create(request.Name, request.Slug, request.Description, request.ParentCategoryId);
        if (categoryResult.IsFailure)
        {
            return Result.Failure<CategoryResponse>(categoryResult.Error);
        }

        var category = categoryResult.Value;
        await _categoryRepository.AddAsync(category, ct);

        return Result.Success(MapToCategoryResponse(category));
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category == null)
        {
            return Result.Failure<CategoryResponse>(Error.NotFound("Category.NotFound", $"Category with ID '{id}' not found."));
        }

        if (request.ParentCategoryId.HasValue && request.ParentCategoryId.Value != id)
        {
            var parentExists = await _categoryRepository.ExistsAsync(request.ParentCategoryId.Value, ct);
            if (!parentExists)
            {
                return Result.Failure<CategoryResponse>(Error.NotFound(
                    "Category.ParentNotFound",
                    $"Parent category with ID '{request.ParentCategoryId.Value}' does not exist."));
            }
        }

        var slugExists = await _categoryRepository.SlugExistsAsync(request.Slug, id, ct);
        if (slugExists)
        {
            return Result.Failure<CategoryResponse>(Error.Conflict(
                "Category.DuplicateSlug",
                $"A category with slug '{request.Slug}' already exists."));
        }

        var updateResult = category.Update(request.Name, request.Slug, request.Description, request.ParentCategoryId, request.IsActive);
        if (updateResult.IsFailure)
        {
            return Result.Failure<CategoryResponse>(updateResult.Error);
        }

        await _categoryRepository.UpdateAsync(category, ct);

        return Result.Success(MapToCategoryResponse(category));
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, ct);
        if (category == null)
        {
            return Result.Failure<CategoryResponse>(Error.NotFound("Category.NotFound", $"Category with ID '{id}' not found."));
        }

        return Result.Success(MapToCategoryResponse(category));
    }

    public async Task<Result<IReadOnlyCollection<CategoryResponse>>> GetCategoriesAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var categories = await _categoryRepository.GetAllAsync(activeOnly, ct);
        var responses = categories.Select(MapToCategoryResponse).ToList();
        return Result.Success<IReadOnlyCollection<CategoryResponse>>(responses);
    }

    #endregion

    #region Brands

    public async Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandRequest request, CancellationToken ct = default)
    {
        var slugExists = await _brandRepository.SlugExistsAsync(request.Slug, null, ct);
        if (slugExists)
        {
            return Result.Failure<BrandResponse>(Error.Conflict(
                "Brand.DuplicateSlug",
                $"A brand with slug '{request.Slug}' already exists."));
        }

        var brandResult = Brand.Create(request.Name, request.Slug, request.Description, request.WebsiteUrl, request.LogoUrl);
        if (brandResult.IsFailure)
        {
            return Result.Failure<BrandResponse>(brandResult.Error);
        }

        var brand = brandResult.Value;
        await _brandRepository.AddAsync(brand, ct);

        return Result.Success(MapToBrandResponse(brand));
    }

    public async Task<Result<BrandResponse>> UpdateBrandAsync(Guid id, UpdateBrandRequest request, CancellationToken ct = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, ct);
        if (brand == null)
        {
            return Result.Failure<BrandResponse>(Error.NotFound("Brand.NotFound", $"Brand with ID '{id}' not found."));
        }

        var slugExists = await _brandRepository.SlugExistsAsync(request.Slug, id, ct);
        if (slugExists)
        {
            return Result.Failure<BrandResponse>(Error.Conflict(
                "Brand.DuplicateSlug",
                $"A brand with slug '{request.Slug}' already exists."));
        }

        var updateResult = brand.Update(request.Name, request.Slug, request.Description, request.WebsiteUrl, request.LogoUrl, request.IsActive);
        if (updateResult.IsFailure)
        {
            return Result.Failure<BrandResponse>(updateResult.Error);
        }

        await _brandRepository.UpdateAsync(brand, ct);

        return Result.Success(MapToBrandResponse(brand));
    }

    public async Task<Result<BrandResponse>> GetBrandByIdAsync(Guid id, CancellationToken ct = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, ct);
        if (brand == null)
        {
            return Result.Failure<BrandResponse>(Error.NotFound("Brand.NotFound", $"Brand with ID '{id}' not found."));
        }

        return Result.Success(MapToBrandResponse(brand));
    }

    public async Task<Result<IReadOnlyCollection<BrandResponse>>> GetBrandsAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var brands = await _brandRepository.GetAllAsync(activeOnly, ct);
        var responses = brands.Select(MapToBrandResponse).ToList();
        return Result.Success<IReadOnlyCollection<BrandResponse>>(responses);
    }

    #endregion

    #region Products

    public async Task<Result<ProductDetailResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category == null)
        {
            return Result.Failure<ProductDetailResponse>(Error.NotFound(
                "Category.NotFound",
                $"Category with ID '{request.CategoryId}' not found."));
        }

        var brand = await _brandRepository.GetByIdAsync(request.BrandId, ct);
        if (brand == null)
        {
            return Result.Failure<ProductDetailResponse>(Error.NotFound(
                "Brand.NotFound",
                $"Brand with ID '{request.BrandId}' not found."));
        }

        var slugExists = await _productRepository.SlugExistsAsync(request.Slug, null, ct);
        if (slugExists)
        {
            return Result.Failure<ProductDetailResponse>(Error.Conflict(
                "Product.DuplicateSlug",
                $"A product with slug '{request.Slug}' already exists."));
        }

        var priceResult = Money.Create(request.BasePrice, request.Currency ?? "USD");
        if (priceResult.IsFailure)
        {
            return Result.Failure<ProductDetailResponse>(priceResult.Error);
        }

        var productResult = Product.Create(
            request.Name,
            request.Slug,
            request.Description,
            request.CategoryId,
            request.BrandId,
            priceResult.Value);

        if (productResult.IsFailure)
        {
            return Result.Failure<ProductDetailResponse>(productResult.Error);
        }

        var product = productResult.Value;

        if (request.Images != null)
        {
            foreach (var img in request.Images)
            {
                var imgResult = product.AddImage(img.Url, img.AltText, img.DisplayOrder, img.IsMain);
                if (imgResult.IsFailure)
                {
                    return Result.Failure<ProductDetailResponse>(imgResult.Error);
                }
            }
        }

        if (request.Variants != null)
        {
            foreach (var v in request.Variants)
            {
                var skuExists = await _productRepository.SkuExistsAsync(v.Sku, ct);
                if (skuExists)
                {
                    return Result.Failure<ProductDetailResponse>(Error.Conflict(
                        "Product.DuplicateSku",
                        $"A variant with SKU '{v.Sku}' already exists in the system."));
                }

                var variantPriceResult = Money.Create(v.Price, v.Currency ?? request.Currency ?? "USD");
                if (variantPriceResult.IsFailure)
                {
                    return Result.Failure<ProductDetailResponse>(variantPriceResult.Error);
                }

                var addVariantResult = product.AddVariant(v.Sku, v.Barcode, variantPriceResult.Value, v.StockQuantity, v.Attributes);
                if (addVariantResult.IsFailure)
                {
                    return Result.Failure<ProductDetailResponse>(addVariantResult.Error);
                }
            }
        }

        product.Publish();

        await _productRepository.AddAsync(product, ct);

        return Result.Success(MapToProductDetailResponse(product, category, brand));
    }

    public async Task<Result<ProductDetailResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product == null)
        {
            return Result.Failure<ProductDetailResponse>(Error.NotFound("Product.NotFound", $"Product with ID '{id}' not found."));
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category == null)
        {
            return Result.Failure<ProductDetailResponse>(Error.NotFound("Category.NotFound", $"Category with ID '{request.CategoryId}' not found."));
        }

        var brand = await _brandRepository.GetByIdAsync(request.BrandId, ct);
        if (brand == null)
        {
            return Result.Failure<ProductDetailResponse>(Error.NotFound("Brand.NotFound", $"Brand with ID '{request.BrandId}' not found."));
        }

        var slugExists = await _productRepository.SlugExistsAsync(request.Slug, id, ct);
        if (slugExists)
        {
            return Result.Failure<ProductDetailResponse>(Error.Conflict(
                "Product.DuplicateSlug",
                $"A product with slug '{request.Slug}' already exists."));
        }

        var priceResult = Money.Create(request.BasePrice, request.Currency ?? "USD");
        if (priceResult.IsFailure)
        {
            return Result.Failure<ProductDetailResponse>(priceResult.Error);
        }

        var updateResult = product.Update(
            request.Name,
            request.Slug,
            request.Description,
            request.CategoryId,
            request.BrandId,
            priceResult.Value,
            request.ExpectedVersion);

        if (updateResult.IsFailure)
        {
            return Result.Failure<ProductDetailResponse>(updateResult.Error);
        }

        await _productRepository.UpdateAsync(product, ct);

        return Result.Success(MapToProductDetailResponse(product, category, brand));
    }

    public async Task<Result<ProductDetailResponse>> GetProductByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product == null)
        {
            return Result.Failure<ProductDetailResponse>(Error.NotFound("Product.NotFound", $"Product with ID '{id}' not found."));
        }

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, ct);
        var brand = await _brandRepository.GetByIdAsync(product.BrandId, ct);

        return Result.Success(MapToProductDetailResponse(product, category, brand));
    }

    public async Task<Result<PagedResult<ProductListResponse>>> SearchProductsAsync(ProductQueryParameters parameters, CancellationToken ct = default)
    {
        var paged = await _productRepository.SearchAsync(parameters, ct);
        return Result.Success(paged);
    }

    public async Task<Result<ProductVariantResponse>> AddVariantAsync(Guid productId, ProductVariantDto request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct);
        if (product == null)
        {
            return Result.Failure<ProductVariantResponse>(Error.NotFound("Product.NotFound", $"Product with ID '{productId}' not found."));
        }

        var skuExists = await _productRepository.SkuExistsAsync(request.Sku, ct);
        if (skuExists)
        {
            return Result.Failure<ProductVariantResponse>(Error.Conflict(
                "Product.DuplicateSku",
                $"A variant with SKU '{request.Sku}' already exists."));
        }

        var priceResult = Money.Create(request.Price, request.Currency ?? product.BasePrice.Currency);
        if (priceResult.IsFailure)
        {
            return Result.Failure<ProductVariantResponse>(priceResult.Error);
        }

        var addResult = product.AddVariant(request.Sku, request.Barcode, priceResult.Value, request.StockQuantity, request.Attributes);
        if (addResult.IsFailure)
        {
            return Result.Failure<ProductVariantResponse>(addResult.Error);
        }

        await _productRepository.UpdateAsync(product, ct);

        return Result.Success(MapToVariantResponse(addResult.Value));
    }

    public async Task<Result<ProductVariantResponse>> UpdateVariantStockAsync(Guid productId, Guid variantId, StockAdjustmentRequest request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct);
        if (product == null)
        {
            return Result.Failure<ProductVariantResponse>(Error.NotFound("Product.NotFound", $"Product with ID '{productId}' not found."));
        }

        var updateResult = product.UpdateVariantStock(variantId, request.NewQuantity);
        if (updateResult.IsFailure)
        {
            return Result.Failure<ProductVariantResponse>(updateResult.Error);
        }

        await _productRepository.UpdateAsync(product, ct);

        return Result.Success(MapToVariantResponse(updateResult.Value));
    }

    public async Task<Result<bool>> ArchiveProductAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product == null)
        {
            return Result.Failure<bool>(Error.NotFound("Product.NotFound", $"Product with ID '{id}' not found."));
        }

        var archiveResult = product.Archive();
        if (archiveResult.IsFailure)
        {
            return Result.Failure<bool>(archiveResult.Error);
        }

        await _productRepository.UpdateAsync(product, ct);
        return Result.Success(true);
    }

    #endregion

    #region Helper Mappers

    private static CategoryResponse MapToCategoryResponse(Category category)
    {
        var subCats = category.SubCategories?.Select(MapToCategoryResponse).ToList();
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            category.IsActive,
            subCats);
    }

    private static BrandResponse MapToBrandResponse(Brand brand)
    {
        return new BrandResponse(
            brand.Id,
            brand.Name,
            brand.Slug,
            brand.Description,
            brand.WebsiteUrl,
            brand.LogoUrl,
            brand.IsActive);
    }

    private static ProductVariantResponse MapToVariantResponse(ProductVariant variant)
    {
        return new ProductVariantResponse(
            variant.Id,
            variant.Sku,
            variant.Barcode,
            variant.Price.Amount,
            variant.Price.Currency,
            variant.StockQuantity,
            variant.IsInStock,
            variant.Attributes);
    }

    private static ProductImageResponse MapToImageResponse(ProductImage image)
    {
        return new ProductImageResponse(
            image.Id,
            image.Url,
            image.AltText,
            image.DisplayOrder,
            image.IsMain);
    }

    private static ProductDetailResponse MapToProductDetailResponse(Product product, Category? category, Brand? brand)
    {
        var catResponse = category != null ? MapToCategoryResponse(category) : null;
        var brandResponse = brand != null ? MapToBrandResponse(brand) : null;
        var images = product.Images.Select(MapToImageResponse).ToList();
        var variants = product.Variants.Select(MapToVariantResponse).ToList();

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Status.ToString(),
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.Version,
            catResponse,
            brandResponse,
            images,
            variants,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }

    #endregion
}
