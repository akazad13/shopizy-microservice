namespace Shopizy.SharedKernel.Domain;

/// <summary>
/// Base exception for domain invariant rule violations.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public DomainException(string code, string message, Exception innerException) : base(message, innerException)
    {
        Code = code;
    }
}
