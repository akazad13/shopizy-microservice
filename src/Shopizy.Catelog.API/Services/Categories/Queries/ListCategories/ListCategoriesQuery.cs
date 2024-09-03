using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Categories;

namespace Shopizy.Catelog.API.Services.Categories.Queries.ListCategories;

public record ListCategoriesQuery() : IRequest<ErrorOr<List<Category>>>;
