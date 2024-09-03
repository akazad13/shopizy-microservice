using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Persistence.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Queries.ListCategories;

public class ListCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<ListCategoriesQuery, ErrorOr<List<Category>>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<List<Category>>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        return await _categoryRepository.GetCategoriesAsync();
    }
}
