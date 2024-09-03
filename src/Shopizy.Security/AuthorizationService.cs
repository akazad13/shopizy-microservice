using ErrorOr;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;
using Shopizy.Security.User;

namespace Shopizy.Security;

public class AuthorizationService(IPolicyEnforcer policyEnforcer, ICurrentUserProvider currentUserProvider) : IAuthorizationService
{
    private readonly IPolicyEnforcer _policyEnforcer = policyEnforcer;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;

    public ErrorOr<Success> AuthorizeCurrentUser<T>(IAuthorizeableRequest<T> request, List<string> requiredRoles, List<string> requiredPermissions, List<string> requiredPolicies)
    {
        CurrentUser? currentUser = _currentUserProvider.GetCurrentUser();

        if (currentUser == null)
        {
            return Error.Unauthorized(description: "User is unauthorized.");
        }

        if (requiredPermissions.Except(currentUser.Permissions).Any())
        {
            return Error.Unauthorized(description: "User is missing required permissions for taking this action");
        }

        if (requiredRoles.Except(currentUser.Roles).Any())
        {
            return Error.Unauthorized(description: "User is missing required roles for taking this action");
        }

        foreach (string policy in requiredPolicies)
        {
            ErrorOr<Success> authorizationAgaistPolicyResult = _policyEnforcer.Authorize(request, currentUser, policy);
            if (authorizationAgaistPolicyResult.IsError)
            {
                return authorizationAgaistPolicyResult.Errors;
            }
        }
        return Result.Success;
    }
}
