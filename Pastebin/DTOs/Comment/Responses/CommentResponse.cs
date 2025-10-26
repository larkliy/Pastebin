
namespace Pastebin.DTOs.Comment.Responses;

public record CommentResponse(Guid id, string content, DateTime createdAt, int upvotes, int downvotes, string? username, string? avatarUrl)
{
    public Guid Id { get; init; } = id;
    public string Content { get; init; } = content;
    public DateTime CreatedAt { get; init; } = createdAt;
    public int Upvotes { get; init; } = upvotes;
    public int Downvotes { get; init; } = downvotes;
    public string? Username { get; init; } = username;
    public string? AvatarUrl { get; init; } = avatarUrl;
    public ICollection<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
}