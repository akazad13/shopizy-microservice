using Shopizy.PaymentService.Domain.Exceptions;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.PaymentService.Domain.ValueObjects;

public sealed class PaymentMethod : ValueObject
{
    public string Token { get; }
    public string Brand { get; }
    public string Last4 { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public PaymentMethod(string token, string brand = "Visa", string last4 = "4242")
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new PaymentDomainException("PaymentMethod.InvalidToken", "Payment token must not be empty.");

        Token = token.Trim();
        Brand = string.IsNullOrWhiteSpace(brand) ? "Unknown" : brand.Trim();
        Last4 = string.IsNullOrWhiteSpace(last4) ? "0000" : last4.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Token;
        yield return Brand;
        yield return Last4;
    }
}
