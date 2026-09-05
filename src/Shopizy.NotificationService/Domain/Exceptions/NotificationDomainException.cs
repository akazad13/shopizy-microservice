namespace Shopizy.NotificationService.Domain.Exceptions;

public sealed class NotificationDomainException : Exception
{
    public string Code { get; }

    public NotificationDomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}
