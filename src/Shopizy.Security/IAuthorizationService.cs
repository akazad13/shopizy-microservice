using ErrorOr;
using Shopizy.Security.Request;

namespace Shopizy.Security;

public interface IAuthorizationService
{
    ErrorOr<Success> AuthorizeCurrentUser<T>(
        IAuthorizeableRequest<T> request,
        IList<string> requiredRoles,
        IList<string> requiredPermissions,
        IList<string> requiredPolicies
    );
}
