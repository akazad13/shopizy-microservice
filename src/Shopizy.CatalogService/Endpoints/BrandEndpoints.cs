using Microsoft.AspNetCore.Mvc;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Endpoints;

public static class BrandEndpoints
{
    public static IEndpointRouteBuilder MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/catalog/brands")
            .WithTags("Brands");

        group.MapGet("/", async (
            [FromQuery] bool? activeOnly,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.GetBrandsAsync(activeOnly ?? true, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .WithName("GetBrands")
        .Produces<IReadOnlyCollection<BrandResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (
            Guid id,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.GetBrandByIdAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .WithName("GetBrandById")
        .Produces<BrandResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            [FromBody] CreateBrandRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.CreateBrandAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/catalog/brands/{result.Value.Id}", result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("CreateBrand")
        .Produces<BrandResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateBrandRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.UpdateBrandAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("UpdateBrand")
        .Produces<BrandResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return app;
    }
}
