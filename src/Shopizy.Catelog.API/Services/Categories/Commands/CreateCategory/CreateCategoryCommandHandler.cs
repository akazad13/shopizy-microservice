using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Errors;
using Shopizy.Catelog.API.Persistence.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        : IRequestHandler<CreateCategoryCommand, ErrorOr<Category>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<ErrorOr<Category>> Handle(
        CreateCategoryCommand cmd,
        CancellationToken cancellationToken
    )
    {
        if (await _categoryRepository.GetCategoryByNameAsync(cmd.Name))
            return CustomErrors.Category.DuplicateName;

        var category = Category.Create(cmd.Name, cmd.ParentId);
        await _categoryRepository.AddAsync(category);

        if (await _categoryRepository.Commit(cancellationToken) <= 0)
            return CustomErrors.Category.CategoryNotCreated;

        return category;
    }
}
