using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.DTOs.Like.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Exceptions.Like;
using Pastebin.Models;
using Pastebin.Services.Interfaces;

namespace Pastebin.Services.Implementations;

public class LikeService(AppDbContext db, ILogger<LikeService> logger) : ILikeService
{
    public async Task<bool> LikeExistsAsync(Guid userId, Guid pasteId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if like with user ID '{UserId}' and paste ID '{PasteId}' exists.", userId, pasteId);
        return await db.Likes.AnyAsync(l => l.UserId == userId && l.PasteId == pasteId, cancellationToken);
    }

    public async Task<PaginatedResponse<LikeResponse>> GetLikesByPasteIdAsync(Guid pasteId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var likesQuery = db.Likes
            .Where(l => l.PasteId == pasteId)
            .OrderByDescending(l => l.CreatedAt);

        var likes = await likesQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize + 1)
            .Select(l => new LikeResponse(l.Id, l.UserId, l.PasteId, l.CreatedAt))
            .ToListAsync(cancellationToken);

        var hasNextPage = likes.Count > pageSize;
        var items = hasNextPage ? likes.Take(pageSize) : likes;

        logger.LogInformation("Retrieved page {PageNumber} of likes for paste ID '{PasteId}' with page size {PageSize}", pageNumber, pasteId, pageSize);

        return new(items, pageNumber, pageSize, hasNextPage);
    }

    public async Task<PaginatedResponse<LikeResponse>> GetLikesByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var likesQuery = db.Likes
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt);

        var likes = await likesQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize + 1)
            .Select(l => new LikeResponse(l.Id, l.UserId, l.PasteId, l.CreatedAt))
            .ToListAsync(cancellationToken);

        var hasNextPage = likes.Count > pageSize;
        var items = hasNextPage ? likes.Take(pageSize) : likes;

        logger.LogInformation("Retrieved page {PageNumber} of likes for user ID '{UserId}' with page size {PageSize}", pageNumber, userId, pageSize);

        return new(items, pageNumber, pageSize, hasNextPage);
    }

    public async Task<LikeCreateResponse> LikePasteAsync(Guid userId, Guid pasteId, CancellationToken cancellationToken = default)
    {
        if (await LikeExistsAsync(userId, pasteId, cancellationToken))
        {
            logger.LogWarning("Attempted to create like with user ID '{UserId}' and paste ID '{PasteId}' that already exists.", userId, pasteId);
            throw new LikeAlreadyExistsException($"Like with user ID '{userId}' and paste ID '{pasteId}' already exists.");
        }

        var like = new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasteId = pasteId,
            CreatedAt = DateTime.UtcNow
        };

        await db.Likes.AddAsync(like, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created new like with ID '{LikeId}'", like.Id);

        return new(like.Id, like.UserId, like.PasteId, like.CreatedAt);
    }

    public async Task DeleteLikeAsync(Guid userId, Guid pasteId, CancellationToken cancellationToken = default)
    {
        var like = await db.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.PasteId == pasteId, cancellationToken);

        if (like == null)
        {
            logger.LogWarning("Attempted to delete non-existent like with user ID '{UserId}' and paste ID '{PasteId}'", userId, pasteId);
            throw new LikeNotFoundException($"Like with user ID '{userId}' and paste ID '{pasteId}' not found.");
        }

        db.Likes.Remove(like);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted like with ID '{LikeId}'", like.Id);
    }
}