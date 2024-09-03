namespace Shopizy.Cart.API.ApiContracts;

public record RemoveProductFromCartRequest(IList<Guid> ProductIds);
