using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Pastebin.ConfigurationSettings;
using Pastebin.Services;

namespace Pastebin.Tests.UnitTests;

public class JwtServiceTests
{
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly Mock<ILogger<JwtService>> _loggerMock;
    private readonly IJwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        _loggerMock = new Mock<ILogger<JwtService>>();
        
        var jwtSettings = new JwtSettings
        {
            Issuer = "test_issuer",
            Audience = "test_audience",
            Key = "super_secret_key_that_is_long_enough",
            ExpiryInHours = 1
        };
        _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

        _jwtService = new JwtService(_jwtSettingsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = await _jwtService.GenerateTokenAsync(userId, username);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var validationResult = await _jwtService.ValidateTokenAsync(token);
        validationResult.Should().NotBeNull();
        validationResult!.Identity.Should().NotBeNull();
        validationResult.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldContainCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = await _jwtService.GenerateTokenAsync(userId, username);
        var principal = await _jwtService.ValidateTokenAsync(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        var claims = principal.Claims.ToList();
        
        claims.Should().Contain(c => c.Type == "sub" && c.Value == userId.ToString());
        claims.Should().Contain(c => c.Type == "unique_name" && c.Value == username);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var invalidToken = "invalid_token";

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => _jwtService.ValidateTokenAsync(invalidToken));
    }
    
    [Fact]
    public async Task ValidateTokenAsync_WithExpiredToken_ShouldThrowException()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Issuer = "test_issuer",
            Audience = "test_audience",
            Key = "super_secret_key_that_is_long_enough",
            ExpiryInHours = -1 // Expired
        };
        _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);
        var expiredJwtService = new JwtService(_jwtSettingsMock.Object, _loggerMock.Object);
        
        var token = await expiredJwtService.GenerateTokenAsync(Guid.NewGuid(), "test");

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => _jwtService.ValidateTokenAsync(token));
    }
}