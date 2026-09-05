using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Shopizy.CartService.Application.Common.Interfaces;
using Shopizy.CartService.Domain.Entities;
using Shopizy.CartService.Domain.ValueObjects;

namespace Shopizy.CartService.Infrastructure.Redis;

/// <summary>Redis-backed implementation of ICartRepository using IDistributedCache.</summary>
public sealed class RedisCartRepository : ICartRepository
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IDistributedCache _cache;

    public RedisCartRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<Cart?> GetAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(cartId, cancellationToken);
        if (string.IsNullOrEmpty(json)) return null;

        var dto = JsonSerializer.Deserialize<CartDto>(json, JsonOpts);
        if (dto is null) return null;

        var items = dto.Items.Select(i => new CartItem(
            i.ProductId,
            i.VariantId,
            i.ProductName,
            i.VariantSku,
            i.Attributes,
            i.Quantity,
            Money.Create(i.SnapshotAmount, i.SnapshotCurrency),
            i.AddedAtUtc)).ToList();

        return Cart.Restore(dto.Id, dto.CustomerId, items, dto.UpdatedAtUtc);
    }

    public async Task SaveAsync(Cart cart, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var dto = new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            UpdatedAtUtc = cart.UpdatedAtUtc,
            Items = cart.Items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                VariantSku = i.VariantSku,
                Attributes = new Dictionary<string, string>(i.Attributes),
                Quantity = i.Quantity,
                SnapshotAmount = i.SnapshotPrice.Amount,
                SnapshotCurrency = i.SnapshotPrice.Currency,
                AddedAtUtc = i.AddedAtUtc
            }).ToList()
        };

        var json = JsonSerializer.Serialize(dto, JsonOpts);
        var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        await _cache.SetStringAsync(cart.Id, json, opts, cancellationToken);
    }

    public Task DeleteAsync(string cartId, CancellationToken cancellationToken = default)
        => _cache.RemoveAsync(cartId, cancellationToken);

    // ─── Internal DTO shapes ─────────────────────────────────────────────────
    private sealed class CartDto
    {
        public string Id { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
    }

    private sealed class CartItemDto
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantSku { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = [];
        public int Quantity { get; set; }
        public decimal SnapshotAmount { get; set; }
        public string SnapshotCurrency { get; set; } = "USD";
        public DateTimeOffset AddedAtUtc { get; set; }
    }
}
