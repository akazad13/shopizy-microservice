using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Products;
using Shopizy.Domain.Models.Base;

namespace Shopizy.Catelog.API.Aggregates.Categories;

public class Category : AggregateRoot<CategoryId, Guid>
{
    private readonly IList<Product> _products = [];
    public string Name { get; private set; }
    public Guid? ParentId { get; private set; }
    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    public static Category Create(string name, Guid? parentId)
    {
        return new Category(CategoryId.CreateUnique(), name, parentId);
    }

    public void Update(string name, Guid? parentId)
    {
        Name = name;
        ParentId = parentId;
    }

    private Category(CategoryId categoryId, string name, Guid? parentId) : base(categoryId)
    {
        Name = name;
        ParentId = parentId;
    }

    private Category() { }
}

