using Microsoft.AspNetCore.Mvc;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/catalog/products")
            .WithTags("Products");

        // Public Search & Filter
        group.MapGet("/", async (
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid? brandId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? inStockOnly,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortBy,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var parameters = new ProductQueryParameters(
                categoryId,
                brandId,
                minPrice,
                maxPrice,
                inStockOnly,
                searchTerm,
                sortBy,
                page ?? 1,
                pageSize ?? 10);

            var result = await catalogService.SearchProductsAsync(parameters, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .WithName("SearchProducts")
        .Produces<PagedResult<ProductListResponse>>(StatusCodes.Status200OK);

        // Public Product Details
        group.MapGet("/{id:guid}", async (
            Guid id,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.GetProductByIdAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .WithName("GetProductById")
        .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // StoreAdmin Create Product
        group.MapPost("/", async (
            [FromBody] CreateProductRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.CreateProductAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/catalog/products/{result.Value.Id}", result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("CreateProduct")
        .Produces<ProductDetailResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // StoreAdmin Update Product
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateProductRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.UpdateProductAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("UpdateProduct")
        .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // StoreAdmin Archive Product
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.ArchiveProductAsync(id, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("ArchiveProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // StoreAdmin Add Variant
        group.MapPost("/{id:guid}/variants", async (
            Guid id,
            [FromBody] ProductVariantDto request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.AddVariantAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/catalog/products/{id}/variants/{result.Value.Id}", result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("AddProductVariant")
        .Produces<ProductVariantResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // StoreAdmin Adjust Stock
        group.MapPut("/{id:guid}/variants/{variantId:guid}/stock", async (
            Guid id,
            Guid variantId,
            [FromBody] StockAdjustmentRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.UpdateVariantStockAsync(id, variantId, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("UpdateVariantStock")
        .Produces<ProductVariantResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }
}
