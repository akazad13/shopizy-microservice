namespace Shopizy.CartService.Application.DTOs;

public sealed record MoneyDto(decimal Amount, string Currency = "USD");

public sealed record AddToCartRequest(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantSku,
    Dictionary<string, string>? Attributes,
    int Quantity,
    MoneyDto UnitPrice);

public sealed record UpdateCartItemRequest(int Quantity);

public sealed record MergeCartRequest(string GuestCartId);

public sealed record CartItemResponse(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantSku,
    IReadOnlyDictionary<string, string> Attributes,
    int Quantity,
    MoneyDto SnapshotPrice,
    MoneyDto? CurrentCatalogPrice,
    bool HasPriceChanged,
    decimal PriceDifference,
    MoneyDto LineTotal,
    DateTimeOffset AddedAtUtc);

public sealed record CartResponse(
    string CartId,
    Guid? CustomerId,
    IReadOnlyList<CartItemResponse> Items,
    int TotalItemsCount,
    MoneyDto Subtotal,
    bool HasAnyPriceDiscrepancy,
    DateTimeOffset UpdatedAtUtc);
