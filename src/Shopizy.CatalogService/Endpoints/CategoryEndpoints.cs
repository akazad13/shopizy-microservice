using Microsoft.AspNetCore.Mvc;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/catalog/categories")
            .WithTags("Categories");

        group.MapGet("/", async (
            [FromQuery] bool? activeOnly,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.GetCategoriesAsync(activeOnly ?? true, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .WithName("GetCategories")
        .Produces<IReadOnlyCollection<CategoryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (
            Guid id,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.GetCategoryByIdAsync(id, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .WithName("GetCategoryById")
        .Produces<CategoryResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            [FromBody] CreateCategoryRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.CreateCategoryAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/catalog/categories/{result.Value.Id}", result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("CreateCategory")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateCategoryRequest request,
            ICatalogService catalogService,
            CancellationToken ct) =>
        {
            var result = await catalogService.UpdateCategoryAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : EndpointHelpers.ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("UpdateCategory")
        .Produces<CategoryResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return app;
    }
}
