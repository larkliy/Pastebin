
using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.DTOs.Paste.Requests;
using Pastebin.DTOs.Paste.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Exceptions.Paste;
using Pastebin.Models;

namespace Pastebin.Services;

public class PasteService(AppDbContext db, ILogger<PasteService> logger) : IPasteService
{
    public async Task<PasteCreateResponse> CreatePasteAsync(Guid? userId, PasteCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IsPrivate && string.IsNullOrWhiteSpace(request.Password))
            throw new PasswordRequiredException("Password is required for private pastes.");

        var paste = new Paste
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            IsPrivate = request.IsPrivate,
            PasswordHash = request.IsPrivate ? BCrypt.Net.BCrypt.HashPassword(request.Password) : null,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await db.Pastes.AddAsync(paste, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created new paste with ID '{PasteId}'", paste.Id);

        return new(paste.Id, paste.Title, paste.IsPrivate, paste.CreatedAt);
    }

    public async Task<PaginatedResponse<PasteResponse>> GetPastesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var totalPastes = await db.Pastes.CountAsync(cancellationToken);

        var pastes = await db.Pastes
            .AsNoTracking()
            .Where(p => !p.IsPrivate)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PasteResponse(p.Id, p.Title, p.IsPrivate, p.CreatedAt))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved page {PageNumber} of pastes with page size {PageSize}", pageNumber, pageSize);

        return new(pastes, totalPastes, pageNumber, pageSize);

    }

    public async Task<PaginatedResponse<PasteResponse>> GetPastesByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var totalPastes = await db.Pastes.CountAsync(p => p.UserId == userId, cancellationToken);

        var pastes = await db.Pastes
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PasteResponse(p.Id, p.Title, p.IsPrivate, p.CreatedAt))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved page {PageNumber} of pastes for user ID '{UserId}' with page size {PageSize}", pageNumber, userId, pageSize);

        return new(pastes, totalPastes, pageNumber, pageSize);
    }

    public async Task<PasteDetailsResponse> GetPasteDetailsAsync(Guid pasteId, Guid? userId, string? password, CancellationToken cancellationToken = default)
    {
        var paste = await db.Pastes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pasteId, cancellationToken);

        if (paste == null)
        {
            logger.LogWarning("Attempted to retrieve non-existent paste with ID '{PasteId}'", pasteId);
            throw new PasteNotFoundException($"Paste with ID '{pasteId}' not found.");
        }

        if (paste.IsPrivate)
        {
            var isOwner = userId.HasValue && paste.UserId.HasValue && paste.UserId.Value == userId.Value;

            var passwordMatch = !string.IsNullOrEmpty(password) &&
                                !string.IsNullOrEmpty(paste.PasswordHash) &&
                                BCrypt.Net.BCrypt.Verify(password, paste.PasswordHash);

            if (!isOwner && !passwordMatch)
            {
                logger.LogWarning("Failed attempt to access private paste '{PasteId}'.", pasteId);
                throw new PasteNotFoundException($"Paste with ID '{pasteId}' not found.");
            }
        }

        logger.LogInformation("Retrieved details for paste with ID '{PasteId}'", pasteId);

        return new(paste.Id, paste.Title, paste.Content, paste.IsPrivate, paste.CreatedAt, paste.UserId);
    }

    public async Task UpdatePasteAsync(Guid pasteId, Guid userId, PasteUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var paste = await db.Pastes.FindAsync([pasteId], cancellationToken);

        if (paste == null)
        {
            logger.LogWarning("Attempted to update non-existent paste with ID '{PasteId}'", pasteId);
            throw new PasteNotFoundException($"Paste with ID '{pasteId}' not found.");
        }

        if (paste.UserId != userId)
        {
            logger.LogWarning("User '{UserId}' attempted to update paste '{PasteId}' owned by another user '{OwnerId}'.", userId, pasteId, paste.UserId);
            throw new PasteNotFoundException($"Paste with ID '{pasteId}' not found.");
        }

        paste.Title = request.Title ?? paste.Title;
        paste.Content = request.Content ?? paste.Content;

        if (request.IsPrivate.HasValue && request.IsPrivate.Value != paste.IsPrivate)
        {
            paste.IsPrivate = request.IsPrivate.Value;

            if (paste.IsPrivate && string.IsNullOrWhiteSpace(request.Password))
            {
                throw new PasswordRequiredException("Password is required to make a paste private.");
            }

            paste.PasswordHash = paste.IsPrivate ? BCrypt.Net.BCrypt.HashPassword(request.Password) : null;
        }
        else if (paste.IsPrivate && !string.IsNullOrWhiteSpace(request.Password))
        {
            paste.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated paste with ID '{PasteId}'", pasteId);
    }

    public async Task DeletePasteAsync(Guid pasteId, Guid userId, CancellationToken cancellationToken = default)
    {
        var paste = await db.Pastes.FindAsync([pasteId], cancellationToken);

        if (paste == null)
        {
            logger.LogWarning("Attempted to delete non-existent paste with ID '{PasteId}'", pasteId);
            throw new PasteNotFoundException($"Paste with ID '{pasteId}' not found.");
        }

        if (paste.UserId != userId)
        {
            logger.LogWarning("User '{UserId}' attempted to delete paste '{PasteId}' owned by another user '{OwnerId}'.", userId, pasteId, paste.UserId);
            throw new PasteNotFoundException($"Paste with ID '{pasteId}' not found.");
        }

        db.Pastes.Remove(paste);
        await db.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Deleted paste with ID '{PasteId}'", pasteId);
    }
}