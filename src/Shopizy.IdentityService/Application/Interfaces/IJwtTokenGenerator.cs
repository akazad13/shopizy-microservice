using Shopizy.IdentityService.Domain.Entities;

namespace Shopizy.IdentityService.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
