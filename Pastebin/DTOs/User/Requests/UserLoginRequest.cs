using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.User.Requests;

public record UserLoginRequest(
    [Required] [StringLength(30)] string Username,
    [Required] [StringLength(30)] string Password);
