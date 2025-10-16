
namespace Pastebin.DTOs.Comment.Responses;

public class CommentResponse
{
    public Guid Id { get; init; }
    public string Content { get; init; }
    public DateTime CreatedAt { get; init; }
    public int Upvotes { get; init; }
    public int Downvotes { get; init; }
    public string? Username { get; init; }
    public string? AvatarUrl { get; init; }
    public ICollection<CommentResponse> Replies { get; set; } = new List<CommentResponse>();

    public CommentResponse(Guid id, string content, DateTime createdAt, int upvotes, int downvotes, string? username, string? avatarUrl)
    {
        Id = id;
        Content = content;
        CreatedAt = createdAt;
        Upvotes = upvotes;
        Downvotes = downvotes;
        Username = username;
        AvatarUrl = avatarUrl;
    }
}