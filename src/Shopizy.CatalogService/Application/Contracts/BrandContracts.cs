namespace Shopizy.CatalogService.Application.Contracts;

public sealed record CreateBrandRequest(
    string Name,
    string Slug,
    string? Description = null,
    string? WebsiteUrl = null,
    string? LogoUrl = null);

public sealed record UpdateBrandRequest(
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    string? LogoUrl,
    bool IsActive);

public sealed record BrandResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    string? LogoUrl,
    bool IsActive);
