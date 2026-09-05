using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.PaymentService.Domain.Entities;
using Shopizy.PaymentService.Domain.Enums;
using Shopizy.PaymentService.Domain.ValueObjects;
using Shopizy.PaymentService.Infrastructure.Persistence;
using Shopizy.PaymentService.Infrastructure.Persistence.Repositories;

namespace Shopizy.PaymentService.IntegrationTests;

public sealed class PaymentPersistenceTests
{
    private static PaymentDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task AddPayment_CanBeRetrievedByIdAndOrderId()
    {
        using var db = CreateInMemoryDb();
        var repo = new PaymentRepository(db);

        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var tx = PaymentTransaction.Create(paymentId, orderId, customerId, Money.Create(75m), new PaymentMethod("tok_visa"));

        await repo.AddAsync(tx);

        var fetched = await repo.GetByIdAsync(paymentId);
        fetched.Should().NotBeNull();
        fetched!.OrderId.Should().Be(orderId);
        fetched.Status.Should().Be(PaymentStatus.Initiated);

        var byOrder = await repo.GetByOrderIdAsync(orderId);
        byOrder.Should().NotBeNull();
        byOrder!.Id.Should().Be(paymentId);
    }

    [Fact]
    public async Task UpdatePayment_WithRefund_PersistsRefundRecord()
    {
        using var db = CreateInMemoryDb();
        var repo = new PaymentRepository(db);

        var tx = PaymentTransaction.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.Create(100m), new PaymentMethod("tok_visa"));
        tx.MarkSucceeded("ch_persist_test");
        await repo.AddAsync(tx);

        tx.ApplyRefund("re_persist_test", Money.Create(100m), "CustomerCancel");
        await repo.UpdateAsync(tx);

        var fetched = await repo.GetByIdAsync(tx.Id);
        fetched!.Status.Should().Be(PaymentStatus.Refunded);
        fetched.Refund.Should().NotBeNull();
        fetched.Refund!.RefundReference.Should().Be("re_persist_test");
    }
}
