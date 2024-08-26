using ErrorOr;
using MediatR;
using Shopizy.Catelog.API.Aggregates.Products;

namespace Shopizy.Catelog.API.Services.Products.Queries.GetProduct;

public record GetProductQuery(Guid ProductId) : IRequest<ErrorOr<Product?>>;
