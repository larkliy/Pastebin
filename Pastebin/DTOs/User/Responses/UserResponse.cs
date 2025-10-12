namespace Pastebin.DTOs.User.Responses;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt);

