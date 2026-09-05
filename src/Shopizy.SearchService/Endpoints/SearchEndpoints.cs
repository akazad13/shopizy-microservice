using Microsoft.AspNetCore.Mvc;
using Shopizy.SearchService.Application.Contracts;
using Shopizy.SearchService.Application.Services;
using Shopizy.SearchService.Domain.ValueObjects;

namespace Shopizy.SearchService.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/search")
            .WithTags("Search");

        group.MapGet("/", async (
            [FromQuery] string? q,
            [FromQuery] string? category,
            [FromQuery] string? brand,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] double? minRating,
            [FromQuery] bool? inStockOnly,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? sortBy,
            SearchApplicationService searchService,
            CancellationToken ct) =>
        {
            int validatedPage = Math.Max(1, page ?? 1);
            int validatedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

            var query = new SearchQuery
            {
                QueryText = q,
                Category = category,
                Brand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                InStockOnly = inStockOnly,
                Page = validatedPage,
                PageSize = validatedPageSize,
                SortBy = sortBy
            };

            var result = await searchService.SearchAsync(query, ct);
            return Results.Ok(result);
        }).AllowAnonymous();

        group.MapPost("/index", async (
            [FromBody] IndexProductRequest request,
            SearchApplicationService searchService,
            CancellationToken ct) =>
        {
            await searchService.IndexProductAsync(request, ct);
            return Results.Ok(new { message = "Product indexed successfully", productId = request.Id });
        }).RequireAuthorization("StoreAdminOnly");

        group.MapDelete("/index/{productId:guid}", async (
            Guid productId,
            SearchApplicationService searchService,
            CancellationToken ct) =>
        {
            await searchService.DeleteProductAsync(productId, ct);
            return Results.NoContent();
        }).RequireAuthorization("StoreAdminOnly");

        return app;
    }
}
