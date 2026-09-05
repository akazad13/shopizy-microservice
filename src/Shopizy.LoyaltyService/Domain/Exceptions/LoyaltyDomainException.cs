namespace Shopizy.LoyaltyService.Domain.Exceptions;

public class LoyaltyDomainException : Exception
{
    public string Code { get; }

    public LoyaltyDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}
