namespace Shopizy.ShippingService.Domain.Exceptions;

public sealed class ShippingDomainException : Exception
{
    public string Code { get; }

    public ShippingDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}
