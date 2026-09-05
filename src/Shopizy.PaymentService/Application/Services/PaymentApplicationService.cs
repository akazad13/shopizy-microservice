using Shopizy.PaymentService.Application.Contracts;
using Shopizy.PaymentService.Application.Interfaces;
using Shopizy.PaymentService.Domain.Entities;
using Shopizy.PaymentService.Domain.Enums;
using Shopizy.PaymentService.Domain.Exceptions;
using Shopizy.PaymentService.Domain.ValueObjects;

namespace Shopizy.PaymentService.Application.Services;

public sealed class PaymentApplicationService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IPaymentGatewayProvider _gateway;

    public PaymentApplicationService(IPaymentRepository paymentRepo, IPaymentGatewayProvider gateway)
    {
        _paymentRepo = paymentRepo;
        _gateway = gateway;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(Guid customerId, ProcessPaymentRequest request, CancellationToken ct = default)
    {
        // Check if order already has an active/succeeded payment
        var existing = await _paymentRepo.GetByOrderIdAsync(request.OrderId, ct);
        if (existing is not null && existing.Status == PaymentStatus.Succeeded)
            throw new PaymentDomainException("Payment.OrderAlreadyPaid", "Order has already been paid successfully.");

        var money = Money.Create(request.Amount.Amount, request.Amount.Currency);
        var paymentMethod = new PaymentMethod(request.PaymentToken, request.CardBrand, request.Last4);

        var paymentId = Guid.NewGuid();
        var transaction = PaymentTransaction.Create(paymentId, request.OrderId, customerId, money, paymentMethod);

        // Process charge through gateway
        var chargeResult = await _gateway.ChargeAsync(request.PaymentToken, money, ct);

        if (chargeResult.IsSuccess)
        {
            transaction.MarkSucceeded(chargeResult.GatewayTransactionId!);
        }
        else
        {
            transaction.MarkFailed(chargeResult.ErrorMessage ?? "CardDeclined");
        }

        await _paymentRepo.AddAsync(transaction, ct);

        return ToResponse(transaction);
    }

    public async Task<PaymentResponse> RefundPaymentAsync(Guid paymentId, Guid? requestingCustomerId, bool isAdmin, RefundPaymentRequest request, CancellationToken ct = default)
    {
        var transaction = await _paymentRepo.GetByIdAsync(paymentId, ct)
            ?? throw new PaymentDomainException("Payment.NotFound", $"Payment transaction {paymentId} not found.");

        if (!isAdmin && transaction.CustomerId != requestingCustomerId)
            throw new PaymentDomainException("Payment.Unauthorized", "Cannot refund another customer's payment.");

        var refundMoney = request.Amount.HasValue
            ? Money.Create(request.Amount.Value, transaction.Amount.Currency)
            : transaction.Amount;

        // Process refund via gateway
        var refundResult = await _gateway.RefundAsync(transaction.GatewayTransactionId ?? "mock_gw", refundMoney, request.Reason, ct);
        if (!refundResult.IsSuccess)
            throw new PaymentDomainException("Payment.GatewayRefundFailed", refundResult.ErrorMessage ?? "Gateway refund rejected.");

        transaction.ApplyRefund(refundResult.RefundReference!, refundMoney, request.Reason);
        await _paymentRepo.UpdateAsync(transaction, ct);

        return ToResponse(transaction);
    }

    public async Task<PaymentResponse?> GetPaymentAsync(Guid id, Guid? requestingCustomerId, bool isAdmin, CancellationToken ct = default)
    {
        var transaction = await _paymentRepo.GetByIdAsync(id, ct);
        if (transaction is null) return null;

        if (!isAdmin && transaction.CustomerId != requestingCustomerId)
            return null; // Isolation

        return ToResponse(transaction);
    }

    public async Task<PaymentResponse?> GetPaymentByOrderIdAsync(Guid orderId, Guid? requestingCustomerId, bool isAdmin, CancellationToken ct = default)
    {
        var transaction = await _paymentRepo.GetByOrderIdAsync(orderId, ct);
        if (transaction is null) return null;

        if (!isAdmin && transaction.CustomerId != requestingCustomerId)
            return null;

        return ToResponse(transaction);
    }

    public async Task<IReadOnlyList<PaymentResponse>> ListPaymentsAsync(Guid? requestingCustomerId, bool isAdmin, CancellationToken ct = default)
    {
        IReadOnlyList<PaymentTransaction> list;
        if (isAdmin && !requestingCustomerId.HasValue)
        {
            list = await _paymentRepo.GetAllAsync(ct);
        }
        else if (requestingCustomerId.HasValue)
        {
            list = await _paymentRepo.GetByCustomerIdAsync(requestingCustomerId.Value, ct);
        }
        else
        {
            list = [];
        }

        return list.Select(ToResponse).ToList();
    }

    public static PaymentResponse ToResponse(PaymentTransaction tx)
    {
        RefundResponse? refundDto = tx.Refund is null ? null : new RefundResponse(
            tx.Refund.Id,
            tx.Refund.RefundReference,
            new MoneyDto(tx.Refund.Amount.Amount, tx.Refund.Amount.Currency),
            tx.Refund.Reason,
            tx.Refund.CreatedAtUtc);

        return new PaymentResponse(
            tx.Id,
            tx.OrderId,
            tx.CustomerId,
            tx.Status.ToString(),
            new MoneyDto(tx.Amount.Amount, tx.Amount.Currency),
            tx.PaymentMethod.Brand,
            tx.PaymentMethod.Last4,
            tx.GatewayTransactionId,
            tx.FailureReason,
            tx.CreatedAtUtc,
            tx.SucceededAtUtc,
            tx.RefundedAtUtc,
            refundDto);
    }
}
