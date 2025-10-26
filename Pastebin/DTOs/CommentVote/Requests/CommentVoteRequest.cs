using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.CommentVote.Requests;

public record CommentVoteRequest(
    [Required] Guid CommentId,
    [Required] bool IsUpvote
);
