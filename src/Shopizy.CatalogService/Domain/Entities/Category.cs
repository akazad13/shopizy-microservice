using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Results;

namespace Shopizy.CatalogService.Domain.Entities;

public sealed class Category : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    // EF Navigation
    private readonly List<Category> _subCategories = [];
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private Category() { }

    private Category(Guid id, string name, string slug, string? description, Guid? parentCategoryId) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Result<Category> Create(string name, string slug, string? description = null, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Category>(Error.Validation("Category.EmptyName", "Category name is required."));
        }

        if (name.Length > 100)
        {
            return Result.Failure<Category>(Error.Validation("Category.NameTooLong", "Category name cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Category>(Error.Validation("Category.EmptySlug", "Category slug is required."));
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return Result.Success(new Category(Guid.NewGuid(), name.Trim(), normalizedSlug, description?.Trim(), parentCategoryId));
    }

    public Result<Category> Update(string name, string slug, string? description, Guid? parentCategoryId, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Category>(Error.Validation("Category.EmptyName", "Category name is required."));
        }

        if (name.Length > 100)
        {
            return Result.Failure<Category>(Error.Validation("Category.NameTooLong", "Category name cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Category>(Error.Validation("Category.EmptySlug", "Category slug is required."));
        }

        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
        {
            return Result.Failure<Category>(Error.Validation("Category.SelfReferencingParent", "A category cannot be its own parent."));
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim();
        ParentCategoryId = parentCategoryId;
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
