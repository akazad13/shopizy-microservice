namespace Shopizy.CartAbandonmentWorker.Domain.Exceptions;

public class CartAbandonmentDomainException : Exception
{
    public string Code { get; }

    public CartAbandonmentDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}
