using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Persistence.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Queries.GetCategory;

public class GetCategoryQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryQuery, ErrorOr<Category?>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<Category?>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        return await _categoryRepository.GetCategoryByIdAsync(CategoryId.Create(query.CategoryId));
    }
}
