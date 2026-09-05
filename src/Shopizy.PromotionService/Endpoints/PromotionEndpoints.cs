using Microsoft.AspNetCore.Mvc;
using Shopizy.PromotionService.Application.Contracts;
using Shopizy.PromotionService.Application.Services;

namespace Shopizy.PromotionService.Endpoints;

public static class PromotionEndpoints
{
    public static IEndpointRouteBuilder MapPromotionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/promotions")
            .WithTags("Promotions");

        group.MapPost("/apply", async (
            [FromBody] ApplyPromotionRequest request,
            PromotionApplicationService promoService,
            CancellationToken ct) =>
        {
            var result = await promoService.ApplyPromotionAsync(request, ct);
            return Results.Ok(result);
        }).AllowAnonymous();

        group.MapPost("/{code}/use", async (
            string code,
            PromotionApplicationService promoService,
            CancellationToken ct) =>
        {
            await promoService.RecordUsageAsync(code, ct);
            return Results.NoContent();
        }).AllowAnonymous();

        group.MapPost("/campaigns", async (
            [FromBody] CreateCampaignRequest request,
            PromotionApplicationService promoService,
            CancellationToken ct) =>
        {
            var campaign = await promoService.CreateCampaignAsync(request, ct);
            return Results.Created($"/api/v1/promotions/campaigns/{campaign.Id}", campaign);
        }).RequireAuthorization("StoreAdminOnly");

        group.MapGet("/campaigns", async (
            PromotionApplicationService promoService,
            CancellationToken ct) =>
        {
            var list = await promoService.GetCampaignsAsync(ct);
            return Results.Ok(list);
        }).RequireAuthorization("StoreAdminOnly");

        group.MapDelete("/campaigns/{id:guid}", async (
            Guid id,
            PromotionApplicationService promoService,
            CancellationToken ct) =>
        {
            await promoService.DeactivateCampaignAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("StoreAdminOnly");

        return app;
    }
}
