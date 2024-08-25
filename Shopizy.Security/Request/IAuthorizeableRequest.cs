using MediatR;

namespace Shopizy.Security.Request;

public interface IAuthorizeableRequest<out T> : IRequest<T>
{
    Guid UserId { get; }
}
