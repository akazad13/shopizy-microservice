namespace Shopizy.Cart.API.ApiContracts;

public record UpdateProductQuantityRequest(Guid ProductId, int Quantity);
