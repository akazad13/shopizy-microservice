using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Products;

namespace Shopizy.Catelog.API.Services.Products.Queries.ListProducts;

public record ListProductQuery() : IRequest<ErrorOr<List<Product>?>>;
