using FluentValidation;

namespace Shopizy.Catelog.API.Services.Categories.Commands.DeleteCategory;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator() { }
}
