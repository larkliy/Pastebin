namespace Pastebin.Models;

public class Comment
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid PasteId { get; set; }
    public Paste? Paste { get; set; }

    public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
}
