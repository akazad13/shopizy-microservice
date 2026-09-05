using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shopizy.PaymentService.Application.Contracts;
using Shopizy.PaymentService.Application.Services;
using Shopizy.PaymentService.Domain.Exceptions;

namespace Shopizy.PaymentService.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments").WithTags("Payments");

        // POST /api/v1/payments
        group.MapPost("/", async (
            [FromBody] ProcessPaymentRequest req,
            HttpContext ctx,
            PaymentApplicationService paymentSvc,
            CancellationToken ct) =>
        {
            var (customerId, _) = ResolveUser(ctx);
            if (!customerId.HasValue) return Results.Unauthorized();

            try
            {
                var response = await paymentSvc.ProcessPaymentAsync(customerId.Value, req, ct);
                return response.Status == "Succeeded"
                    ? Results.Created($"/api/v1/payments/{response.Id}", response)
                    : Results.Problem(detail: response.FailureReason, statusCode: 400, title: "Payment Failed");
            }
            catch (PaymentDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        }).RequireAuthorization();

        // GET /api/v1/payments/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            HttpContext ctx,
            PaymentApplicationService paymentSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            if (!customerId.HasValue && !isAdmin) return Results.Unauthorized();

            var response = await paymentSvc.GetPaymentAsync(id, customerId, isAdmin, ct);
            return response is null ? Results.NotFound() : Results.Ok(response);
        }).RequireAuthorization();

        // GET /api/v1/payments
        group.MapGet("/", async (
            HttpContext ctx,
            PaymentApplicationService paymentSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            if (!customerId.HasValue && !isAdmin) return Results.Unauthorized();

            var list = await paymentSvc.ListPaymentsAsync(customerId, isAdmin, ct);
            return Results.Ok(list);
        }).RequireAuthorization();

        // POST /api/v1/payments/{id}/refund
        group.MapPost("/{id:guid}/refund", async (
            Guid id,
            [FromBody] RefundPaymentRequest req,
            HttpContext ctx,
            PaymentApplicationService paymentSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            if (!customerId.HasValue && !isAdmin) return Results.Unauthorized();

            try
            {
                var response = await paymentSvc.RefundPaymentAsync(id, customerId, isAdmin, req, ct);
                return Results.Ok(response);
            }
            catch (PaymentDomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400, title: ex.Code);
            }
        }).RequireAuthorization();

        // GET /api/v1/payments/order/{orderId}
        group.MapGet("/order/{orderId:guid}", async (
            Guid orderId,
            HttpContext ctx,
            PaymentApplicationService paymentSvc,
            CancellationToken ct) =>
        {
            var (customerId, isAdmin) = ResolveUser(ctx);
            if (!customerId.HasValue && !isAdmin) return Results.Unauthorized();

            var response = await paymentSvc.GetPaymentByOrderIdAsync(orderId, customerId, isAdmin, ct);
            return response is null ? Results.NotFound() : Results.Ok(response);
        }).RequireAuthorization();

        return app;
    }

    private static (Guid? customerId, bool isAdmin) ResolveUser(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated != true)
            return (null, false);

        var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? ctx.User.FindFirstValue("sub");
        var role = ctx.User.FindFirstValue(ClaimTypes.Role)
                ?? ctx.User.FindFirstValue("role");

        bool isAdmin = string.Equals(role, "StoreAdmin", StringComparison.OrdinalIgnoreCase);
        Guid? customerId = Guid.TryParse(sub, out var id) ? id : null;

        return (customerId, isAdmin);
    }
}
