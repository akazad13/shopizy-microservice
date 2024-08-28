namespace Shopizy.Cart.API.ApiContracts;

public record CartResponse(
    Guid CartId,
    Guid CustomerId,
    DateTime CreatedOn,
    DateTime ModifiedOn,
    List<CartItemResponse> CartItems
);

public record CartItemResponse(
    Guid CartItemId,
    Guid ProductId,
    int Quantity,
    ProductResponse Product
);

public record ProductResponse(
    string Name,
    string Description,
    string Price,
    decimal Discount,
    string Brand,
    int StockQuantity,
    List<string>? ProductImages
);
