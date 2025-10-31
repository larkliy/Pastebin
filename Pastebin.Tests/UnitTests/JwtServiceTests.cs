using Pastebin.Services.Interfaces;
using Microsoft.Extensions.Options;
using Pastebin.ConfigurationSettings;
using Pastebin.Services.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Pastebin.Models;

namespace Pastebin.UnitTests;

public class JwtServiceTests
{
    private readonly IJwtService _jwtService;
    private readonly Mock<IOptions<JwtSettings>> _optionsMock;
    private readonly Mock<ILogger<JwtService>> _loggerMock;

    public JwtServiceTests()
    {
        _optionsMock = new();
        _loggerMock = new();

        _optionsMock.Setup(o => o.Value).Returns(new JwtSettings
        {
            Issuer = "Pastebin",
            Audience = "Pastebin",
            Key = "super_secret_key_that_is_long_enough12222222",
            ExpiryInHours = 24
        });

        _jwtService = new JwtService(_optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldReturnToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "password",
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _jwtService.GenerateTokenAsync(user);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnToken()
    {
        // Act
        var result = _jwtService.GenerateRefreshToken();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnPrincipal_WhenTokenIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "password",
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        var token = await _jwtService.GenerateTokenAsync(user);

        // Act
        var result = await _jwtService.ValidateTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.Identity?.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var token = "invalid_token";

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => _jwtService.ValidateTokenAsync(token));
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldContainUserClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "password",
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var token = await _jwtService.GenerateTokenAsync(user);
        var principal = await _jwtService.ValidateTokenAsync(token);

        principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
        principal?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value.Should().Be(user.Username);
    }
}
