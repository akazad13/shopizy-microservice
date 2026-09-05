using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shopizy.IdentityService.Application.Contracts;
using Shopizy.IdentityService.Application.Interfaces;
using Shopizy.SharedKernel.Results;

namespace Shopizy.IdentityService.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity")
            .WithTags("Identity");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var result = await identityService.RegisterAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/identity/me", result.Value)
                : ToProblemDetails(result.Error);
        })
        .WithName("Register")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var result = await identityService.LoginAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : ToProblemDetails(result.Error);
        })
        .WithName("Login")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var result = await identityService.RefreshTokenAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : ToProblemDetails(result.Error);
        })
        .WithName("RefreshToken")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value
                    ?? user.FindFirst("role")?.Value
                    ?? "Customer";

            var result = await identityService.GetProfileAsync(userId, userId, role, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : ToProblemDetails(result.Error);
        })
        .RequireAuthorization()
        .WithName("GetProfile")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/users/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var requestingUserIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(requestingUserIdStr, out var requestingUserId))
            {
                return Results.Unauthorized();
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value
                    ?? user.FindFirst("role")?.Value
                    ?? "Customer";

            var result = await identityService.GetProfileAsync(id, requestingUserId, role, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : ToProblemDetails(result.Error);
        })
        .RequireAuthorization()
        .WithName("GetUserById")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/users", async (
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var result = await identityService.GetUserDirectoryAsync(ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : ToProblemDetails(result.Error);
        })
        .RequireAuthorization("StoreAdminOnly")
        .WithName("GetUserDirectory")
        .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static IResult ToProblemDetails(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        return Results.Problem(problemDetails);
    }
}
