namespace Pastebin.DTOs.User.Responses;

public record UserCreateResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt);
