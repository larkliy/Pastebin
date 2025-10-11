
namespace Pastebin.Models;



public class User
{
    public Guid Id { get; set; }

    public required string Username { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Paste> Pastes { get; set; } = new List<Paste>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
}


