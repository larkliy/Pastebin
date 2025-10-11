namespace Pastebin.DTOs.User.Responses;

public record UserLoginResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt,
    string AccessToken, 
    string RefreshToken);
