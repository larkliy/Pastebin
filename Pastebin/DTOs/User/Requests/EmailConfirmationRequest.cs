
using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.User.Requests;

public record EmailConfirmationRequest(
    [Required][EmailAddress] string Email,
    [Required] string EmailConfirmationToken);