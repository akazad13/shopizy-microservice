using ErrorOr;
using MediatR;

namespace Shopizy.Catelog.API.Services.Products.Queries.ProductAvailability;

public record ProductAvailabilityQuery(Guid ProductId) : IRequest<ErrorOr<bool>>;
