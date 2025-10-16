using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.Paste.Requests;

public record PasteCreateRequest(
    [Required][MinLength(1)][MaxLength(100)] string Title,
    [Required][MinLength(1)][MaxLength(5000)] string Content,
    [Required] bool IsPrivate,
    [MinLength(8)] string? Password);