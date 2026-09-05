using FluentAssertions;
using Moq;
using Shopizy.IdentityService.Application.Interfaces;
using Shopizy.IdentityService.Application.Services;
using Shopizy.IdentityService.Domain.Entities;
using Shopizy.IdentityService.Domain.Enums;
using Shopizy.IdentityService.Domain.ValueObjects;

namespace Shopizy.IdentityService.UnitTests.Services;

public sealed class IdentityServiceDataIsolationTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Application.Services.IdentityService _identityService;

    public IdentityServiceDataIsolationTests()
    {
        _identityService = new Application.Services.IdentityService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WhenCustomerAccessesOwnProfile_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("customer@shopizy.test").Value;
        var user = User.Create("Alice", "Smith", email, "hashed-pass", UserRole.Customer);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act - Customer requesting own profile
        var result = await _identityService.GetProfileAsync(
            targetUserId: userId,
            requestingUserId: userId,
            requestingUserRole: UserRole.Customer.ToString());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("customer@shopizy.test");
    }

    [Fact]
    public async Task GetProfileAsync_WhenCustomerAttemptsToAccessAnotherUserProfile_ReturnsForbidden()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var customerId = Guid.NewGuid(); // Different ID

        // Act - Customer attempting to inspect another user's profile
        var result = await _identityService.GetProfileAsync(
            targetUserId: targetUserId,
            requestingUserId: customerId,
            requestingUserRole: UserRole.Customer.ToString());

        // Assert - Constitution Principle V: Data Isolation
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.Forbidden");
        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetProfileAsync_WhenStoreAdminAccessesAnotherUserProfile_ReturnsSuccess()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var email = Email.Create("customer@shopizy.test").Value;
        var user = User.Create("Alice", "Smith", email, "hashed-pass", UserRole.Customer);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act - StoreAdmin accessing customer profile
        var result = await _identityService.GetProfileAsync(
            targetUserId: targetUserId,
            requestingUserId: adminId,
            requestingUserRole: UserRole.StoreAdmin.ToString());

        // Assert - Admins have authority over user directory
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("customer@shopizy.test");
    }
}
