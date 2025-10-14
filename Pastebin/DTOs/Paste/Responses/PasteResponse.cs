
namespace Pastebin.DTOs.Paste.Responses;

public record PasteResponse(
    Guid Id,
    string Title,
    bool IsPrivate,
    DateTime CreatedAt);