using Mapster;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Services.Categories.Commands.CreateCategory;
using Shopizy.Catelog.API.Services.Categories.Commands.DeleteCategory;
using Shopizy.Catelog.API.Services.Categories.Commands.UpdateCategory;
using Shopizy.Catelog.API.Services.Categories.Queries.GetCategory;
using Shopizy.Contracts.Category;

namespace Shopizy.Catelog.API.Mapping;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<(Guid UserId, CreateCategoryRequest Request), CreateCategoryCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.Request);

        config
            .NewConfig<
                (Guid UserId, Guid CategoryId, UpdateCategoryRequest Request),
                UpdateCategoryCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest, src => src.Request);

        config
            .NewConfig<(Guid UserId, Guid CategoryId), DeleteCategoryCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CategoryId, src => src.CategoryId);

        config.NewConfig<Category, CategoryResponse>().Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<Guid, GetCategoryQuery>().MapWith(src => new GetCategoryQuery(src));
    }
}


// Map(dest => dest.FullName, src => $"{src.Title} {src.FirstName} {src.LastName}")
//       .Map(dest => dest.Age,
//             src => DateTime.Now.Year - src.DateOfBirth.Value.Year,
//             srcCond => srcCond.DateOfBirth.HasValue);
