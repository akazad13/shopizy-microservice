namespace Shopizy.CatalogService.Application.Contracts;

public sealed record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description = null,
    Guid? ParentCategoryId = null);

public sealed record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    IReadOnlyCollection<CategoryResponse>? SubCategories = null);
