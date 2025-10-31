
namespace Pastebin.DTOs.User.Responses;

public record EmailConfirmationResponse(
    string EmailConfirmationToken, 
    DateTime? EmailConfirmationTokenExpiresAt);
