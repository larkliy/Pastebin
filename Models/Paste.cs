namespace Pastebin.Models;

public class Paste
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsPrivate { get; set; }
    public string? PasswordHash { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
