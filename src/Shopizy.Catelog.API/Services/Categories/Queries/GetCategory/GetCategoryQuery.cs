using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Queries.GetCategory;

public record GetCategoryQuery(Guid CategoryId) : IRequest<ErrorOr<Category?>>;
