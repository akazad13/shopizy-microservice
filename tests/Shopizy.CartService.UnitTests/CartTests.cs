using FluentAssertions;
using Shopizy.CartService.Domain.Entities;
using Shopizy.CartService.Domain.ValueObjects;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.CartService.UnitTests;

public sealed class CartTests
{
    private static readonly Guid ProductA = Guid.NewGuid();
    private static readonly Guid VariantA = Guid.NewGuid();
    private static readonly Guid ProductB = Guid.NewGuid();
    private static readonly Guid VariantB = Guid.NewGuid();

    // ─── Cart Creation ────────────────────────────────────────────────────────

    [Fact]
    public void CreateForCustomer_EmptyGuid_ThrowsDomainException()
    {
        var act = () => Cart.CreateForCustomer(Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Customer ID*");
    }

    [Fact]
    public void CreateForGuest_EmptyGuestId_ThrowsDomainException()
    {
        var act = () => Cart.CreateForGuest("");
        act.Should().Throw<DomainException>().WithMessage("*Guest cart ID*");
    }

    [Fact]
    public void CreateForCustomer_ValidId_SetsCartId()
    {
        var id = Guid.NewGuid();
        var cart = Cart.CreateForCustomer(id);
        cart.Id.Should().Be($"cart:customer:{id}");
        cart.CustomerId.Should().Be(id);
        cart.Items.Should().BeEmpty();
    }

    // ─── AddItem ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddItem_ValidItem_AppearsInCart()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var price = Money.Create(29.99m);

        cart.AddItem(ProductA, VariantA, "Headphones", "SKU-001", null, 2, price);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(2);
        cart.Items[0].SnapshotPrice.Amount.Should().Be(29.99m);
        cart.Subtotal.Amount.Should().Be(59.98m);
    }

    [Fact]
    public void AddItem_SameVariantTwice_IncrementsQuantity()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var price = Money.Create(10m);

        cart.AddItem(ProductA, VariantA, "P", "S", null, 2, price);
        cart.AddItem(ProductA, VariantA, "P", "S", null, 3, price);

        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_QuantityZero_ThrowsDomainException()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var act = () => cart.AddItem(ProductA, VariantA, "P", "S", null, 0, Money.Create(10m));
        act.Should().Throw<DomainException>().WithMessage("*Quantity*");
    }

    [Fact]
    public void AddItem_QuantityOver99_ThrowsDomainException()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var act = () => cart.AddItem(ProductA, VariantA, "P", "S", null, 100, Money.Create(10m));
        act.Should().Throw<DomainException>().WithMessage("*Quantity*");
    }

    [Fact]
    public void AddItem_NullProductName_ThrowsDomainException()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var act = () => cart.AddItem(ProductA, VariantA, "", "S", null, 1, Money.Create(10m));
        act.Should().Throw<DomainException>().WithMessage("*Product name*");
    }

    [Fact]
    public void AddItem_EmptyVariantSku_ThrowsDomainException()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var act = () => cart.AddItem(ProductA, VariantA, "P", "  ", null, 1, Money.Create(10m));
        act.Should().Throw<DomainException>().WithMessage("*Variant SKU*");
    }

    [Fact]
    public void AddItem_ExceedsMaxQuantity_CapsAt99()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var price = Money.Create(10m);
        cart.AddItem(ProductA, VariantA, "P", "S", null, 97, price);
        cart.AddItem(ProductA, VariantA, "P", "S", null, 5, price);
        cart.Items[0].Quantity.Should().Be(99);
    }

    // ─── UpdateItemQuantity ───────────────────────────────────────────────────

    [Fact]
    public void UpdateItemQuantity_ValidQty_UpdatesSubtotal()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        cart.AddItem(ProductA, VariantA, "P", "S", null, 2, Money.Create(20m));

        cart.UpdateItemQuantity(VariantA, 5);

        cart.Items[0].Quantity.Should().Be(5);
        cart.Subtotal.Amount.Should().Be(100m);
    }

    [Fact]
    public void UpdateItemQuantity_NonExistentVariant_ThrowsDomainException()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var act = () => cart.UpdateItemQuantity(Guid.NewGuid(), 2);
        act.Should().Throw<DomainException>().WithMessage("*not found*");
    }

    // ─── RemoveItem ───────────────────────────────────────────────────────────

    [Fact]
    public void RemoveItem_ExistingVariant_RemovesFromCart()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        cart.AddItem(ProductA, VariantA, "P", "S", null, 1, Money.Create(10m));
        cart.RemoveItem(VariantA);
        cart.Items.Should().BeEmpty();
        cart.Subtotal.Amount.Should().Be(0m);
    }

    [Fact]
    public void RemoveItem_NonExistentVariant_ThrowsDomainException()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        var act = () => cart.RemoveItem(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*not found*");
    }

    // ─── Clear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        cart.AddItem(ProductA, VariantA, "P", "S", null, 1, Money.Create(10m));
        cart.AddItem(ProductB, VariantB, "Q", "T", null, 2, Money.Create(5m));
        cart.Clear();
        cart.Items.Should().BeEmpty();
        cart.Subtotal.Amount.Should().Be(0m);
    }

    // ─── MergeWith ────────────────────────────────────────────────────────────

    [Fact]
    public void MergeWith_DisjointItems_AdoptsBothSets()
    {
        var customer = Cart.CreateForCustomer(Guid.NewGuid());
        customer.AddItem(ProductA, VariantA, "P", "S", null, 1, Money.Create(10m));

        var guest = Cart.CreateForGuest("guest-1");
        guest.AddItem(ProductB, VariantB, "Q", "T", null, 2, Money.Create(5m));

        customer.MergeWith(guest);

        customer.Items.Should().HaveCount(2);
        customer.Items.First(i => i.VariantId == VariantA).Quantity.Should().Be(1);
        customer.Items.First(i => i.VariantId == VariantB).Quantity.Should().Be(2);
    }

    [Fact]
    public void MergeWith_OverlappingVariants_SumsQuantities()
    {
        var customer = Cart.CreateForCustomer(Guid.NewGuid());
        customer.AddItem(ProductA, VariantA, "P", "S", null, 2, Money.Create(10m));

        var guest = Cart.CreateForGuest("guest-2");
        guest.AddItem(ProductA, VariantA, "P", "S", null, 3, Money.Create(10m));

        customer.MergeWith(guest);

        customer.Items.Should().HaveCount(1);
        customer.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void MergeWith_EmptyGuestCart_CustomerCartUnchanged()
    {
        var customer = Cart.CreateForCustomer(Guid.NewGuid());
        customer.AddItem(ProductA, VariantA, "P", "S", null, 1, Money.Create(10m));
        var guest = Cart.CreateForGuest("guest-3");

        customer.MergeWith(guest);

        customer.Items.Should().HaveCount(1);
    }

    [Fact]
    public void MergeWith_QuantityOverflows99_CapsAt99()
    {
        var customer = Cart.CreateForCustomer(Guid.NewGuid());
        customer.AddItem(ProductA, VariantA, "P", "S", null, 97, Money.Create(1m));

        var guest = Cart.CreateForGuest("guest-4");
        guest.AddItem(ProductA, VariantA, "P", "S", null, 5, Money.Create(1m));

        customer.MergeWith(guest);

        customer.Items[0].Quantity.Should().Be(99);
    }

    // ─── Subtotal ─────────────────────────────────────────────────────────────

    [Fact]
    public void Subtotal_MultipleItems_CorrectSum()
    {
        var cart = Cart.CreateForCustomer(Guid.NewGuid());
        cart.AddItem(ProductA, VariantA, "P", "S", null, 2, Money.Create(20m));  // 40
        cart.AddItem(ProductB, VariantB, "Q", "T", null, 3, Money.Create(10m));  // 30

        cart.Subtotal.Amount.Should().Be(70m);
        cart.TotalItemsCount.Should().Be(5);
    }
}
