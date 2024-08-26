using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Shopizy.Security.User;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public CurrentUser? GetCurrentUser()
    {

        if (!_httpContextAccessor.HttpContext!.User.Claims.Any())
            return null;

        var id = Guid.Parse(GetSingleClaimValue("id"));
        var permissions = GetClaimValues("permissions");
        var roles = GetClaimValues(ClaimTypes.Role);
        var firstName = GetSingleClaimValue(JwtRegisteredClaimNames.Name);
        var lastName = GetSingleClaimValue(ClaimTypes.Surname);
        var phone = GetSingleClaimValue(ClaimTypes.MobilePhone);

        return new CurrentUser(id, firstName, lastName, phone, permissions, roles);
    }


    private List<string> GetClaimValues(string claimType) =>
        _httpContextAccessor.HttpContext!.User.Claims
            .Where(claim => claim.Type == claimType)
            .Select(claim => claim.Value)
            .ToList();
    private string GetSingleClaimValue(string claimType) =>
        _httpContextAccessor.HttpContext!.User.Claims
            .Single(claim => claim.Type == claimType)
            .Value;
}