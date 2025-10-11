using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.User.Requests;

public record UpdateUserRequest(
    [MaxLength(30)] string? Username,
    [EmailAddress] string? Email,
    [MaxLength(30)] string? Password);
