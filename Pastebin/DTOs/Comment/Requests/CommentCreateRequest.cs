
using System.ComponentModel.DataAnnotations;

namespace Pastebin.DTOs.Comment.Requests;

public record CommentCreateRequest(
    [Required] [StringLength(300, MinimumLength = 1)] string Content,
    Guid? ParentId);