namespace Pastebin.DTOs.User.Responses;

public record FoundedUserResponse(
    Guid Id,
    string Username,
    DateTime CreatedAt);
