using Ardalis.GuardClauses;
using ErrorOr;
using Shopizy.Security.Request;
using Shopizy.Security.Roles;
using Shopizy.Security.User;

namespace Shopizy.Security.Policies;

public class PolicyEnforcer : IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy
    )
    {
        _ = Guard.Against.Null(request);
        _ = Guard.Against.Null(currentUser);

        return policy switch
        {
            Policy.SelfOrAdmin => SelfOrAdminPolicy(request, currentUser),
            _ => Error.Unexpected(description: "Unknown policy name.")
        };
    }

    private static ErrorOr<Success> SelfOrAdminPolicy<T>(
    IAuthorizeableRequest<T> request,
        CurrentUser currentUser
    )
    {
        return request.UserId == currentUser.Id || currentUser.Roles.Contains(Role.Admin)
            ? Result.Success
            : Error.Unauthorized(description: "Requesting user failed policy requirement.");
    }
}
