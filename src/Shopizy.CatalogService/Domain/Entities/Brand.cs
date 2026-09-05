using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Domain.Entities;

public sealed class Brand : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Brand() { }

    private Brand(Guid id, string name, string slug, string? description, string? websiteUrl, string? logoUrl) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        WebsiteUrl = websiteUrl;
        LogoUrl = logoUrl;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Result<Brand> Create(string name, string slug, string? description = null, string? websiteUrl = null, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Brand>(Error.Validation("Brand.EmptyName", "Brand name is required."));
        }

        if (name.Length > 100)
        {
            return Result.Failure<Brand>(Error.Validation("Brand.NameTooLong", "Brand name cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Brand>(Error.Validation("Brand.EmptySlug", "Brand slug is required."));
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return Result.Success(new Brand(Guid.NewGuid(), name.Trim(), normalizedSlug, description?.Trim(), websiteUrl?.Trim(), logoUrl?.Trim()));
    }

    public Result<Brand> Update(string name, string slug, string? description, string? websiteUrl, string? logoUrl, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Brand>(Error.Validation("Brand.EmptyName", "Brand name is required."));
        }

        if (name.Length > 100)
        {
            return Result.Failure<Brand>(Error.Validation("Brand.NameTooLong", "Brand name cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Brand>(Error.Validation("Brand.EmptySlug", "Brand slug is required."));
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim();
        WebsiteUrl = websiteUrl?.Trim();
        LogoUrl = logoUrl?.Trim();
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success(this);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
