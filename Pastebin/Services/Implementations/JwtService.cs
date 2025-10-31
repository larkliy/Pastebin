using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Pastebin.ConfigurationSettings;
using Pastebin.Models;
using Pastebin.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Pastebin.Services.Implementations;

public class JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger) : IJwtService
{
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public Task<string> GenerateTokenAsync(User user)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("EmailConfirmed", user.IsEmailConfirmed.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = jwtSettings.Value.Issuer,
            Audience = jwtSettings.Value.Audience,
            Expires = DateTime.UtcNow.AddHours(jwtSettings.Value.ExpiryInHours),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Value.Key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        logger.LogInformation("Generating JWT token for user {UserId} with expiry {Expiry} hours", user.Id, jwtSettings.Value.ExpiryInHours);

        return Task.FromResult(_tokenHandler.CreateToken(tokenDescriptor));
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var token = Convert.ToBase64String(randomBytes);

        logger.LogInformation("Generated refresh token");

        return token;
    }

    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Value.Issuer,
            ValidAudience = jwtSettings.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Value.Key))
        };

        var validationResult = await _tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

        if (!validationResult.IsValid
        || validationResult.SecurityToken is not JsonWebToken jsonWebToken
        || jsonWebToken.Alg != SecurityAlgorithms.HmacSha256Signature)
        {
            logger.LogWarning("Invalid JWT token");
            throw new SecurityTokenException("Token is expired or invalid", validationResult.Exception);
        }
            
        var principal = new ClaimsPrincipal(validationResult.ClaimsIdentity);

        logger.LogInformation("JWT Token has been succesfully validated");

        return principal;
    }
}