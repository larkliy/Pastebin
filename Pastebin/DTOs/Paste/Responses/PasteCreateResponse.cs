namespace Pastebin.DTOs.Paste.Responses;

public record PasteCreateResponse(
    Guid Id,
    string Title,
    bool IsPrivate,
    DateTime CreatedAt);
