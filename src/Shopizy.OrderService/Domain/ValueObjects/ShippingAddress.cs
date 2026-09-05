using Shopizy.OrderService.Domain.Exceptions;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.OrderService.Domain.ValueObjects;

public sealed class ShippingAddress : ValueObject
{
    public string FullName { get; }
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public ShippingAddress(string fullName, string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new OrderDomainException("ShippingAddress.InvalidFullName", "Full name must not be empty.");
        if (string.IsNullOrWhiteSpace(street))
            throw new OrderDomainException("ShippingAddress.InvalidStreet", "Street address must not be empty.");
        if (string.IsNullOrWhiteSpace(city))
            throw new OrderDomainException("ShippingAddress.InvalidCity", "City must not be empty.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new OrderDomainException("ShippingAddress.InvalidPostalCode", "Postal code must not be empty.");
        if (string.IsNullOrWhiteSpace(country))
            throw new OrderDomainException("ShippingAddress.InvalidCountry", "Country must not be empty.");

        FullName = fullName.Trim();
        Street = street.Trim();
        City = city.Trim();
        State = (state ?? string.Empty).Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FullName;
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
