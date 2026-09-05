using Shopizy.IdentityService.Application.Contracts;
using Shopizy.IdentityService.Application.Interfaces;
using Shopizy.IdentityService.Domain.Entities;
using Shopizy.IdentityService.Domain.Rules;
using Shopizy.IdentityService.Domain.ValueObjects;
using Shopizy.SharedKernel.Results;

namespace Shopizy.IdentityService.Application.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public IdentityService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthResponse>(emailResult.Error);
        }

        var passwordResult = PasswordPolicy.Validate(request.Password);
        if (passwordResult.IsFailure)
        {
            return Result.Failure<AuthResponse>(passwordResult.Error);
        }

        var emailExists = await _userRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken);
        if (emailExists)
        {
            return Result.Failure<AuthResponse>(
                Error.Conflict("User.AlreadyExists", $"A user with email '{emailResult.Value}' already exists."));
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(
            request.FirstName,
            request.LastName,
            emailResult.Value,
            passwordHash,
            request.Role);

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await _userRepository.AddAsync(user, cancellationToken);

        var (accessToken, expiresAtUtc) = _jwtTokenGenerator.GenerateAccessToken(user);

        var userResponse = new UserResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAtUtc);

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken,
            expiresAtUtc,
            userResponse));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await _userRepository.UpdateAsync(user, cancellationToken);

        var (accessToken, expiresAtUtc) = _jwtTokenGenerator.GenerateAccessToken(user);

        var userResponse = new UserResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAtUtc);

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken,
            expiresAtUtc,
            userResponse));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is required."));
        }

        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidRefreshToken", "Invalid or expired refresh token."));
        }

        var existingToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid, expired, or revoked."));
        }

        // Rotate token
        user.RevokeRefreshToken(request.RefreshToken);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        await _userRepository.UpdateAsync(user, cancellationToken);

        var (accessToken, expiresAtUtc) = _jwtTokenGenerator.GenerateAccessToken(user);

        var userResponse = new UserResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAtUtc);

        return Result.Success(new AuthResponse(
            accessToken,
            newRefreshToken,
            expiresAtUtc,
            userResponse));
    }

    public async Task<Result<UserResponse>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserResponse>(Error.NotFound("User.NotFound", $"User with ID '{userId}' was not found."));
        }

        return Result.Success(new UserResponse(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAtUtc));
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> GetUserDirectoryAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<UserResponse> responseList = users.Select(u => new UserResponse(
            u.Id,
            u.Email.Value,
            u.FirstName,
            u.LastName,
            u.Role.ToString(),
            u.CreatedAtUtc)).ToList();

        return Result.Success(responseList);
    }
}
