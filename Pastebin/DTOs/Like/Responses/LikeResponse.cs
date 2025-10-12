
namespace Pastebin.DTOs.Like.Responses;

public record LikeResponse(
    Guid Id,
    Guid UserId,
    Guid PasteId,
    DateTime CreatedAt);