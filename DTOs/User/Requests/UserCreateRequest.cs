using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.User.Requests;

public record UserCreateRequest(
    [Required] [StringLength(30, MinimumLength = 3)] string Username,
    [Required] [EmailAddress] string Email,
    [Required] [StringLength(100, MinimumLength = 6)] string Password);