using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.Persistence.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
        : IRequestHandler<UpdateCategoryCommand, ErrorOr<Category>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<Category>> Handle(
        UpdateCategoryCommand cmd,
        CancellationToken cancellationToken
    )
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(CategoryId.Create(cmd.CategoryId));
        if (category is null)
            return CustomErrors.Category.CategoryNotFound;

        category.Update(cmd.Name, cmd.ParentId);

        _categoryRepository.Update(category);

        if (await _categoryRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Category.CategoryNotUpdated;

        return category;
    }
}
