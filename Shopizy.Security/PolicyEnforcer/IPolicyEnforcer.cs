using ErrorOr;
using Shopizy.Security.CurrentUserProvider;
using Shopizy.Security.Request;

namespace Shopizy.Security.PolicyEnforcer;

public interface IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy
    );
}