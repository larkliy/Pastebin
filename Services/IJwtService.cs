using System.Security.Claims;

namespace Pastebin.Services;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(Guid userId, string username);
    string GenerateRefreshToken();
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
}
