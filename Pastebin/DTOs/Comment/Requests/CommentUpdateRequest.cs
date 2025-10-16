using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.Comment.Requests;

public record CommentUpdateRequest(
    [Required][StringLength(300, MinimumLength = 1)] string Content);
