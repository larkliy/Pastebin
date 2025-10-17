namespace Pastebin.DTOs.User.Responses;

public record FoundUserResponseUser(
    Guid Id,
    string Username,
    DateTime CreatedAt);
