namespace Shopizy.PromotionService.Domain.Exceptions;

public sealed class PromotionDomainException : Exception
{
    public string Code { get; }

    public PromotionDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}
