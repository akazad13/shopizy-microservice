using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shopizy.IdentityService.Domain.Entities;
using Shopizy.IdentityService.Domain.Enums;
using Shopizy.IdentityService.Domain.ValueObjects;
using Shopizy.IdentityService.Infrastructure.Persistence;

namespace Shopizy.IdentityService.IntegrationTests.Persistence;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_PersistsUserAndRetrievesById()
    {
        // Arrange
        var email = Email.Create("buyer@shopizy.test").Value;
        var user = User.Create("Bob", "Builder", email, "hashed-password", UserRole.Customer);

        // Act
        await _repository.AddAsync(user);
        var retrieved = await _repository.GetByIdAsync(user.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
        retrieved.Email.Should().Be(email);
        retrieved.FirstName.Should().Be("Bob");
        retrieved.LastName.Should().Be("Builder");
        retrieved.Role.Should().Be(UserRole.Customer);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        // Arrange
        var email = Email.Create("merchant@shopizy.test").Value;
        var user = User.Create("Carol", "Danvers", email, "hashed-admin-pass", UserRole.StoreAdmin);
        await _repository.AddAsync(user);

        // Act
        var retrieved = await _repository.GetByEmailAsync(email);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
        retrieved.Role.Should().Be(UserRole.StoreAdmin);
    }

    [Fact]
    public async Task GetByRefreshTokenAsync_ReturnsUserWithToken()
    {
        // Arrange
        var email = Email.Create("token.user@shopizy.test").Value;
        var user = User.Create("Dave", "Token", email, "hashed-pass");
        const string tokenString = "refresh-token-xyz-12345";
        user.AddRefreshToken(tokenString, DateTime.UtcNow.AddDays(7));

        await _repository.AddAsync(user);

        // Act
        var retrieved = await _repository.GetByRefreshTokenAsync(tokenString);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
        retrieved.RefreshTokens.Should().Contain(rt => rt.Token == tokenString && rt.IsActive);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReturnsTrueWhenUserExists()
    {
        // Arrange
        var email = Email.Create("exists@shopizy.test").Value;
        var user = User.Create("Eva", "Green", email, "hashed-pass");
        await _repository.AddAsync(user);

        // Act
        var exists = await _repository.ExistsByEmailAsync(email);
        var notExists = await _repository.ExistsByEmailAsync(Email.Create("other@shopizy.test").Value);

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }
}
