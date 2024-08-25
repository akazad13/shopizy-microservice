using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Domain.Models.Specifications;

namespace Shopizy.Catelog.API.Persistence.Categories.Specifications;

internal class CategoryByIdSpec(CategoryId id) : Specification<Category>(category => category.Id == id)
{
}
