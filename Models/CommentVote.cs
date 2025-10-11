namespace Pastebin.Models;

public class CommentVote
{
    public Guid Id { get; set; }
    
    public bool IsUpvote { get; set; } 

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
}
