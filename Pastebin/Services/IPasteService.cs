using Pastebin.DTOs.Paste.Requests;
using Pastebin.DTOs.Paste.Responses;
using Pastebin.DTOs.Shared;

namespace Pastebin.Services;

public interface IPasteService
{
    Task<PasteCreateResponse> CreatePasteAsync(Guid? userId, PasteCreateRequest request, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<PasteResponse>> GetPastesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<PasteResponse>> GetPastesByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PasteDetailsResponse> GetPasteDetailsAsync(Guid pasteId, Guid? userId, string? password, CancellationToken cancellationToken = default);
    Task UpdatePasteAsync(Guid pasteId, Guid userId, PasteUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeletePasteAsync(Guid pasteId, Guid userId, CancellationToken cancellationToken = default);
}
