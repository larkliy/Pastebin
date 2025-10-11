namespace Pastebin.Models;

public class Comment
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // Связь с автором комментария
    public Guid UserId { get; set; }
    public User User { get; set; }

    // Связь с пастой
    public Guid PasteId { get; set; }
    public Paste Paste { get; set; }

    // Коллекция голосов за этот комментарий
    public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
}
