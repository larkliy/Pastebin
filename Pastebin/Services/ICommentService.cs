using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;

namespace Pastebin.Services;

public interface ICommentService
{
    Task<CommentResponse> CreateCommentAsync(Guid pasteId, Guid userId, string content, CancellationToken cancellationToken = default);
    Task<CommentResponse> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CommentResponse>> GetCommentsAsync(Guid pasteId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CommentResponse>> GetCommentsByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateCommentAsync(Guid commentId, string content, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
}