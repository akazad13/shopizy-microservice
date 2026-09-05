using Shopizy.SharedKernel.Domain;

namespace Shopizy.PaymentService.Domain.Exceptions;

public sealed class PaymentDomainException : DomainException
{
    public PaymentDomainException(string code, string message) : base(code, message)
    {
    }
}
