using FluentValidation;

namespace Shopizy.Catelog.API.Services.Categories.Commands.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(category => category.Name).NotEmpty().MaximumLength(100);
    }
}

