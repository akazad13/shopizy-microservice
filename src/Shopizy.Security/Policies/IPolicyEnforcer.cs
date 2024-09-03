using ErrorOr;
using Shopizy.Security.Request;
using Shopizy.Security.User;

namespace Shopizy.Security.Policies;

public interface IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy
    );
}
