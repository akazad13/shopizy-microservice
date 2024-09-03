using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.Persistence.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
        : IRequestHandler<DeleteCategoryCommand, ErrorOr<Success>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<Success>> Handle(
        DeleteCategoryCommand cmd,
        CancellationToken cancellationToken
    )
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(CategoryId.Create(cmd.CategoryId));
        if (category is null)
        {
            return CustomErrors.Category.CategoryNotFound;
        }

        _categoryRepository.Remove(category);

        if (await _categoryRepository.Commit(cancellationToken) <= 0)
        {
            return CustomErrors.Category.CategoryNotDeleted;
        }

        return Result.Success;
    }
}
