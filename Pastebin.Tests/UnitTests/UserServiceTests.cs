using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pastebin.Application;
using Pastebin.DTOs.User.Requests;
using Pastebin.Exceptions.User;
using Pastebin.Models;
using Moq;
using Pastebin.Services.Implementations;
using Pastebin.Services.Interfaces;

namespace Pastebin.Tests.UnitTests;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly UserService _userService;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _userService = new UserService(_dbContext, _jwtServiceMock.Object, _loggerMock.Object);
    }

    private async Task SeedDatabaseAsync()
    {
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "testuser1", Email = "test1@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") },
            new User { Id = Guid.NewGuid(), Username = "testuser2", Email = "test2@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password456") }
        };

        await _dbContext.Users.AddRangeAsync(users);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UserExistsAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var result = await _userService.UserExistsAsync("testuser1");

        // Assert
        result.Should().BeTrue();
    }

    public async Task UserExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var result = await _userService.UserExistsAsync("nonexistentuser");

        // Assert
        result.Should().BeFalse();
    }

    public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var result = await _userService.EmailExistsAsync("test1@example.com");

        // Assert
        result.Should().BeTrue();
    }

    public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var result = await _userService.EmailExistsAsync("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    public async Task CreateUserAsync_ShouldCreateUser_WhenUsernameAndEmailAreUnique()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var result = await _userService.CreateUserAsync("newuser", "new@example.com", "password");

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.Email.Should().Be("new@example.com");

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        userInDb.Should().NotBeNull();
    }

    public async Task CreateUserAsync_ShouldThrowUserAlreadyExistsException_WhenUsernameExists()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _userService.CreateUserAsync("testuser1", "new@example.com", "password"));
    }

    public async Task CreateUserAsync_ShouldThrowUserAlreadyExistsException_WhenEmailExists()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _userService.CreateUserAsync("newuser", "test1@example.com", "password"));
    }

    public async Task AuthenticateUserAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        await SeedDatabaseAsync();
        _jwtServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync("access_token");
        _jwtServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh_token");

        // Act
        var result = await _userService.AuthenticateUserAsync("testuser1", "password123");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    public async Task AuthenticateUserAsync_ShouldThrowInvalidCredentialsException_WhenUsernameIsInvalid()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _userService.AuthenticateUserAsync("nonexistentuser", "password123"));
    }

    public async Task AuthenticateUserAsync_ShouldThrowInvalidCredentialsException_WhenPasswordIsInvalid()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _userService.AuthenticateUserAsync("testuser1", "wrongpassword"));
    }

    public async Task RefreshTokenAsync_ShouldReturnNewLoginResponse_WhenTokenIsValid()
    {
        // Arrange
        await SeedDatabaseAsync();
        var user = await _dbContext.Users.FirstAsync();
        user.RefreshToken = "valid_refresh_token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
        await _dbContext.SaveChangesAsync();

        _jwtServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync("new_access_token");
        _jwtServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("new_refresh_token");

        // Act
        var result = await _userService.RefreshTokenAsync("valid_refresh_token");

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");
    }

    public async Task RefreshTokenAsync_ShouldThrowInvalidRefreshTokenException_WhenTokenIsInvalid()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            _userService.RefreshTokenAsync("invalid_refresh_token"));
    }

    public async Task RefreshTokenAsync_ShouldThrowInvalidRefreshTokenException_WhenTokenIsExpired()
    {
        // Arrange
        await SeedDatabaseAsync();
        var user = await _dbContext.Users.FirstAsync();
        user.RefreshToken = "expired_refresh_token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            _userService.RefreshTokenAsync("expired_refresh_token"));
    }

    public async Task GetUsersAsync_ShouldReturnPaginatedUsers()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var result = await _userService.GetUsersAsync(1, 1);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.Items.First().Username.Should().Be("testuser1");
    }

    public async Task UpdateUserByIdAsync_ShouldUpdateUser_WhenDataIsValid()
    {
        // Arrange
        await SeedDatabaseAsync();
        var user = await _dbContext.Users.FirstAsync();
        var request = new UpdateUserRequest(user.Id.ToString(),
            "updateduser",
            "updated@example.com", string.Empty
        );

        // Act
        var result = await _userService.UpdateUserByIdAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("updateduser");
        result.Email.Should().Be("updated@example.com");

        var updatedUserInDb = await _dbContext.Users.FindAsync(user.Id);
        updatedUserInDb.Should().NotBeNull();
        updatedUserInDb!.Username.Should().Be("updateduser");
        BCrypt.Net.BCrypt.Verify("newpassword", updatedUserInDb.PasswordHash).Should().BeTrue();
    }
    
    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
