namespace Pastebin.DTOs.Paste.Requests;

public record PasteUpdateRequest(
    string? Title,
    string? Content,
    bool? IsPrivate,
    string? Password);
