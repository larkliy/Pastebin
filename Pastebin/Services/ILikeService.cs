using Pastebin.DTOs.Like.Responses;
using Pastebin.DTOs.Shared;

namespace Pastebin.Services;

public interface ILikeService
{
    Task<bool> LikeExistsAsync(Guid userId, Guid pasteId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<LikeResponse>> GetLikesByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<LikeResponse>> GetLikesByPasteIdAsync(Guid pasteId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<LikeCreateResponse> LikePasteAsync(Guid userId, Guid pasteId, CancellationToken cancellationToken = default);
    Task DeleteLikeAsync(Guid userId, Guid pasteId, CancellationToken cancellationToken = default);
}