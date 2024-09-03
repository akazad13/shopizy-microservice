using Ardalis.GuardClauses;
using Mapster;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.ApiContracts.Category;
using Shopizy.Catelog.API.Services.Categories.Commands.CreateCategory;
using Shopizy.Catelog.API.Services.Categories.Commands.DeleteCategory;
using Shopizy.Catelog.API.Services.Categories.Commands.UpdateCategory;
using Shopizy.Catelog.API.Services.Categories.Queries.GetCategory;

namespace Shopizy.Catelog.API.Mapping;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        _ = Guard.Against.Null(config);

        _ = config
            .NewConfig<(Guid UserId, CreateCategoryRequest Request), CreateCategoryCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.Request);

        _ = config
            .NewConfig<
                (Guid UserId, Guid CategoryId, UpdateCategoryRequest Request),
                UpdateCategoryCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest, src => src.Request);

        _ = config
            .NewConfig<(Guid UserId, Guid CategoryId), DeleteCategoryCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CategoryId, src => src.CategoryId);

        _ = config.NewConfig<Category, CategoryResponse>().Map(dest => dest.Id, src => src.Id.Value);

        _ = config.NewConfig<Guid, GetCategoryQuery>().MapWith(src => new GetCategoryQuery(src));
    }
}
