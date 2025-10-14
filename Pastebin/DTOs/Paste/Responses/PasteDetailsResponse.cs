namespace Pastebin.DTOs.Paste.Responses;

public record PasteDetailsResponse(
    Guid Id,
    string Title,
    string Content,
    bool IsPrivate,
    DateTime CreatedAt,
    Guid? UserId
);