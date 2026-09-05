namespace Shopizy.ReviewService.Domain.Exceptions;

public class ReviewDomainException : Exception
{
    public string Code { get; }

    public ReviewDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}
