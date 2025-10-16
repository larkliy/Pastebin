using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Exceptions.Comment;
using Pastebin.Exceptions.User;
using Pastebin.Models;

namespace Pastebin.Services;

public class CommentService(AppDbContext db, ILogger<CommentService> logger) : ICommentService
{
    public async Task<CommentResponse> CreateCommentAsync(Guid pasteId, Guid userId, string content, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Attempted to create comment with user ID '{UserId}' and paste ID '{PasteId}' that does not exist.", userId, pasteId);
            throw new UserNotFoundException($"User with ID '{userId}' not found.");
        }

        var comment = new Comment
        {
            PasteId = pasteId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await db.Comments.AddAsync(comment, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created new comment with ID '{CommentId}'", comment.Id);

        return new(comment.Id, comment.Content, comment.CreatedAt, 0, 0, user.Username ?? string.Empty, user.ImageUrl ?? string.Empty);
    }
    
    public async Task<CommentResponse> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments
            .Include(c => c.User)
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            logger.LogWarning("Attempted to get comment with ID '{CommentId}' that does not exist.", commentId);
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found.");
        }
        
        return new(comment.Id, comment.Content, comment.CreatedAt, comment.Votes.Count(v => v.IsUpvote), comment.Votes.Count(v => !v.IsUpvote), comment.User?.Username ?? string.Empty, comment.User?.ImageUrl ?? string.Empty);
    }

    public async Task<PaginatedResponse<CommentResponse>> GetCommentsAsync(Guid pasteId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var totalComments = await db.Comments.CountAsync(c => c.PasteId == pasteId, cancellationToken);

        var comments = await db.Comments
            .Where(c => c.PasteId == pasteId)
            .Include(c => c.User)
            .Include(c => c.Votes)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c =>
                    new CommentResponse(
                        c.Id,
                        c.Content,
                        c.CreatedAt,
                        c.Votes.Count(v => v.IsUpvote),
                        c.Votes.Count(v => !v.IsUpvote),
                        c.User!.Username,
                        c.User.ImageUrl))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved page {PageNumber} of comments for paste ID '{PasteId}' with page size {PageSize}", pageNumber, pasteId, pageSize);

        return new(comments, totalComments, pageNumber, pageSize);
    }

    public async Task<PaginatedResponse<CommentResponse>> GetCommentsByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var count = await db.Comments.CountAsync(c => c.UserId == userId, cancellationToken);

        var comments = await db.Comments
            .Where(c => c.UserId == userId)
            .Include(c => c.User)
            .Include(c => c.Votes)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentResponse(
                            c.Id,
                            c.Content,
                            c.CreatedAt,
                            c.Votes.Count(v => v.IsUpvote),
                            c.Votes.Count(v => !v.IsUpvote),
                            c.User!.Username,
                            c.User.ImageUrl))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved page {PageNumber} of comments for user ID '{UserId}' with page size {PageSize}", pageNumber, userId, pageSize);

        return new(comments, count, pageNumber, pageSize);
    }

    public async Task UpdateCommentAsync(Guid commentId, string content, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments.FindAsync([commentId], cancellationToken);
        if (comment == null)
        {
            logger.LogWarning("Attempted to update comment with ID '{CommentId}' that does not exist.", commentId);
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found.");
        }

        comment.Content = content;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated comment with ID '{CommentId}'", comment.Id);
    }

    public async Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments.FindAsync([commentId], cancellationToken);
        if (comment == null)
        {
            logger.LogWarning("Attempted to delete comment with ID '{CommentId}' that does not exist.", commentId);
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found.");
        }

        db.Comments.Remove(comment);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted comment with ID '{CommentId}'", comment.Id);
    }
}