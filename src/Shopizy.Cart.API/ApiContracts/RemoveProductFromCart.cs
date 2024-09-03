namespace Shopizy.Cart.API.ApiContracts;

public record RemoveProductFromCartRequest(List<Guid> ProductIds);
