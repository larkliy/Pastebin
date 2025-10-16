using Pastebin.DTOs.Comment.Requests;
using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;

namespace Pastebin.Services;

public interface ICommentService
{
    Task<CommentResponse> CreateCommentAsync(Guid pasteId, Guid userId, CommentCreateRequest request, CancellationToken cancellationToken = default);
    Task<CommentResponse> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CommentResponse>> GetCommentsAsync(Guid pasteId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<CommentResponse>> GetCommentsByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateCommentAsync(Guid commentId, Guid currentUserId, CommentUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid commentId, Guid currentUserId, CancellationToken cancellationToken = default);
}