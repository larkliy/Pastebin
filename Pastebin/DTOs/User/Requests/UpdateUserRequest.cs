using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.User.Requests;

public record UpdateUserRequest(
    [Required] Guid Id,
    [MaxLength(30)] string? Username,
    [EmailAddress] string? Email,
    [MaxLength(30)] string? Password,
    [MaxLength(500)] string? ImageUrl);
