using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pastebin.Infrastructure;
using Pastebin.Services.Implementations;
using Pastebin.Services.Interfaces;
using Pastebin.Models;
using FluentAssertions;
using Pastebin.Exceptions.User;
using Pastebin.DTOs.User.Requests;
using Microsoft.Extensions.Options;
using Pastebin.ConfigurationSettings;
using Moq;

namespace Pastebin.Tests.UnitTests;

public class UserServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly UserService _userService;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IOptions<ApplicationSettings>> _applicationSettingsMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new(options);
        _jwtServiceMock = new();
        _loggerMock = new();
        _emailServiceMock = new();
        _applicationSettingsMock = new();

        _jwtServiceMock.Setup(s => s.GenerateTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("access_token");

        _jwtServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh_token");

        _applicationSettingsMock.Setup(s => s.Value).Returns(new ApplicationSettings
        {
            FrontendUrl = "https://localhost:7172"
        });

        _userService = new(
            _dbContext,
            _jwtServiceMock.Object,
            _emailServiceMock.Object,
            _applicationSettingsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_WhenUsernameAndEmailAreUnique()
    {
        // Arrange
        var result = await _userService.CreateUserAsync(
            "newuser", "new@example.com", "password", TestContext.Current.CancellationToken);

        // Assert
        var user = await _dbContext.Users.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        result!.Username.Should().Be("newuser");
        user!.Username.Should().Be("newuser");
        user!.Email.Should().Be("new@example.com");

        BCrypt.Net.BCrypt.Verify("password", user!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrow_WhenUsernameExists()
    {
        // Arrange
        await _userService.CreateUserAsync(
            "testuser1", "new@example.com", "password", TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _userService.CreateUserAsync("testuser1", "new@example.com", "password", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange
        await _userService.CreateUserAsync(
            "testuser1", "test1@example.com", "password123", TestContext.Current.CancellationToken);

        // Act
        var result = await _userService.AuthenticateUserAsync(
            "testuser1", "password123", TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task AuthenticateUserAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        // Arrange
        await _userService.CreateUserAsync(
            "testuser1", "test1@example.com", "password123", TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _userService.AuthenticateUserAsync("testuser1", "wrongpassword", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateUserByIdAsync_ShouldUpdateUser_WhenDataIsValid()
    {
        // Arrange
        await _userService.CreateUserAsync(
            "testuser1", "test1@example.com", "password123", TestContext.Current.CancellationToken);
        var user = await _dbContext.Users.FirstAsync(TestContext.Current.CancellationToken);
        var request = new UpdateUserRequest(
            Username: "updateduser",
            Email: "updated@example.com",
            Password: "newpassword",
            ImageUrl: string.Empty
        );

        // Act
        var result = await _userService.UpdateUserByIdAsync(user.Id, request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("updateduser");
        result.Email.Should().Be("updated@example.com");

        var updatedUserInDb = await _dbContext.Users.FindAsync([user.Id], TestContext.Current.CancellationToken);

        updatedUserInDb!.Should().NotBeNull();
        updatedUserInDb!.Username.Should().Be("updateduser");
        BCrypt.Net.BCrypt.Verify("newpassword", updatedUserInDb.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserByIdAsync_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        await _userService.CreateUserAsync(
            "testuser1", "test1@example.com", "password123", TestContext.Current.CancellationToken);
        var user = await _dbContext.Users.FirstAsync(TestContext.Current.CancellationToken);

        // Act
        await _userService.DeleteUserByIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        (await _dbContext.Users.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnPaginatedUsers()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
            await _userService.CreateUserAsync(
                $"testuser{i}", $"test{i}@example.com", "password", TestContext.Current.CancellationToken);
                
        // Act
        var page1 = await _userService.GetUsersAsync(1, 10, TestContext.Current.CancellationToken);
        var page2 = await _userService.GetUsersAsync(2, 10, TestContext.Current.CancellationToken);

        // Assert
        page1.Items.Should().HaveCount(10);
        page2.Items.Should().HaveCount(5);
        page1.HasNextPage.Should().BeTrue();
        page2.HasNextPage.Should().BeFalse();
    }
}
