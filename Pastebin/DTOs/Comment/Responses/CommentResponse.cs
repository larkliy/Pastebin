
namespace Pastebin.DTOs.Comment.Responses;

public record CommentResponse(
    Guid Id,
    string Content,
    DateTime CreatedAt,
    int Upvotes,
    int Downvotes,
    string? Username,
    string? AvatarUrl);