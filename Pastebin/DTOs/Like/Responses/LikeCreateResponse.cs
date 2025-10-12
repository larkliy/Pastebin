
namespace Pastebin.DTOs.Like.Responses;

public record LikeCreateResponse(
    Guid Id,
    Guid UserId,
    Guid PasteId,
    DateTime CreatedAt);