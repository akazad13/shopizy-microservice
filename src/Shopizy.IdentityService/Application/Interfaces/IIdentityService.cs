using Shopizy.IdentityService.Application.Contracts;
using Shopizy.SharedKernel.Results;

namespace Shopizy.IdentityService.Application.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserResponse>>> GetUserDirectoryAsync(CancellationToken cancellationToken = default);
}
