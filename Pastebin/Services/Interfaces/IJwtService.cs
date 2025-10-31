using System.Security.Claims;
using Pastebin.Models;

namespace Pastebin.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(User user);
    string GenerateRefreshToken();
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
}
