using Shopizy.CartService.Application.Common.Interfaces;
using Shopizy.CartService.Application.DTOs;
using Shopizy.CartService.Domain.Entities;
using Shopizy.CartService.Domain.ValueObjects;

namespace Shopizy.CartService.Application.Services;

/// <summary>Orchestrates cart reads, writes, merges, and price discrepancy enrichment.</summary>
public sealed class CartCommandService
{
    private static readonly TimeSpan CustomerCartTtl = TimeSpan.FromDays(30);
    private static readonly TimeSpan GuestCartTtl = TimeSpan.FromDays(7);

    private readonly ICartRepository _cartRepo;
    private readonly ICatalogPriceService _catalogPrice;

    public CartCommandService(ICartRepository cartRepo, ICatalogPriceService catalogPrice)
    {
        _cartRepo = cartRepo;
        _catalogPrice = catalogPrice;
    }

    public async Task<CartResponse> GetCartAsync(string cartId, CancellationToken ct = default)
    {
        var cart = await _cartRepo.GetAsync(cartId, ct);
        if (cart is null)
        {
            // Return an empty cart if not yet created
            cart = cartId.StartsWith("cart:customer:")
                ? Cart.CreateForCustomer(Guid.Parse(cartId.Replace("cart:customer:", "")))
                : Cart.CreateForGuest(cartId.Replace("cart:guest:", ""));
        }
        return await ToResponseAsync(cart, ct);
    }

    public async Task<CartResponse> AddItemAsync(string cartId, Guid? customerId, bool isCustomer, AddToCartRequest req, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCart(cartId, customerId, ct);
        var price = Money.Create(req.UnitPrice.Amount, req.UnitPrice.Currency);
        cart.AddItem(req.ProductId, req.VariantId, req.ProductName, req.VariantSku, req.Attributes, req.Quantity, price);
        await SaveCart(cart, isCustomer, ct);
        return await ToResponseAsync(cart, ct);
    }

    public async Task<CartResponse> UpdateItemAsync(string cartId, bool isCustomer, Guid variantId, int quantity, CancellationToken ct = default)
    {
        var cart = await _cartRepo.GetAsync(cartId, ct)
            ?? throw new SharedKernel.Domain.DomainException("Cart.NotFound", "Cart not found.");
        cart.UpdateItemQuantity(variantId, quantity);
        await SaveCart(cart, isCustomer, ct);
        return await ToResponseAsync(cart, ct);
    }

    public async Task<CartResponse> RemoveItemAsync(string cartId, bool isCustomer, Guid variantId, CancellationToken ct = default)
    {
        var cart = await _cartRepo.GetAsync(cartId, ct)
            ?? throw new SharedKernel.Domain.DomainException("Cart.NotFound", "Cart not found.");
        cart.RemoveItem(variantId);
        await SaveCart(cart, isCustomer, ct);
        return await ToResponseAsync(cart, ct);
    }

    public async Task ClearCartAsync(string cartId, bool isCustomer, CancellationToken ct = default)
    {
        var cart = await _cartRepo.GetAsync(cartId, ct);
        if (cart is null) return;
        cart.Clear();
        await SaveCart(cart, isCustomer, ct);
    }

    public async Task<CartResponse> MergeGuestCartAsync(Guid customerId, string guestCartId, CancellationToken ct = default)
    {
        var customerCartId = $"cart:customer:{customerId}";
        var customerCart = await GetOrCreateCart(customerCartId, customerId, ct);
        var guestCart = await _cartRepo.GetAsync($"cart:guest:{guestCartId}", ct);

        if (guestCart is not null)
        {
            customerCart.MergeWith(guestCart);
            await _cartRepo.DeleteAsync($"cart:guest:{guestCartId}", ct);
        }

        await SaveCart(customerCart, isCustomer: true, ct);
        return await ToResponseAsync(customerCart, ct);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task<Cart> GetOrCreateCart(string cartId, Guid? customerId, CancellationToken ct)
    {
        return await _cartRepo.GetAsync(cartId, ct)
            ?? (customerId.HasValue
                ? Cart.CreateForCustomer(customerId.Value)
                : Cart.CreateForGuest(cartId.Replace("cart:guest:", "")));
    }

    private Task SaveCart(Cart cart, bool isCustomer, CancellationToken ct)
        => _cartRepo.SaveAsync(cart, isCustomer ? CustomerCartTtl : GuestCartTtl, ct);

    private async Task<CartResponse> ToResponseAsync(Cart cart, CancellationToken ct)
    {
        var itemResponses = new List<CartItemResponse>();
        bool anyDiscrepancy = false;

        foreach (var item in cart.Items)
        {
            var currentPrice = await _catalogPrice.GetCurrentPriceAsync(item.VariantId, ct);
            bool changed = currentPrice.HasValue && currentPrice.Value != item.SnapshotPrice.Amount;
            decimal diff = changed ? currentPrice!.Value - item.SnapshotPrice.Amount : 0m;
            if (changed) anyDiscrepancy = true;

            itemResponses.Add(new CartItemResponse(
                item.ProductId,
                item.VariantId,
                item.ProductName,
                item.VariantSku,
                item.Attributes,
                item.Quantity,
                new MoneyDto(item.SnapshotPrice.Amount, item.SnapshotPrice.Currency),
                currentPrice.HasValue ? new MoneyDto(currentPrice.Value, item.SnapshotPrice.Currency) : null,
                changed,
                diff,
                new MoneyDto(item.LineTotal.Amount, item.LineTotal.Currency),
                item.AddedAtUtc));
        }

        return new CartResponse(
            cart.Id,
            cart.CustomerId,
            itemResponses.AsReadOnly(),
            cart.TotalItemsCount,
            new MoneyDto(cart.Subtotal.Amount, cart.Subtotal.Currency),
            anyDiscrepancy,
            cart.UpdatedAtUtc);
    }
}
