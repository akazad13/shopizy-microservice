using Shopizy.SharedKernel.Domain;

namespace Shopizy.OrderService.Domain.Exceptions;

public sealed class OrderDomainException : DomainException
{
    public OrderDomainException(string code, string message) : base(code, message)
    {
    }
}
