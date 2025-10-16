using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.DTOs.Comment.Requests;
using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Exceptions.Comment;
using Pastebin.Exceptions.User;
using Pastebin.Models;

namespace Pastebin.Services;

public class CommentService(AppDbContext db, ILogger<CommentService> logger) : ICommentService
{
    public async Task<CommentResponse> CreateCommentAsync(Guid pasteId, Guid userId, CommentCreateRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken);
        if (user == null)
        {
            logger.LogWarning("Attempted to create comment with user ID '{UserId}' that does not exist.", userId);
            throw new UserNotFoundException($"User with ID '{userId}' not found.");
        }

        var pasteExists = await db.Pastes.AnyAsync(p => p.Id == pasteId, cancellationToken);
        if (!pasteExists)
        {
            logger.LogWarning("Attempted to create comment on non-existent paste with ID '{PasteId}'.", pasteId);
            throw new CommentNotFoundException($"Paste with ID '{pasteId}' not found.");
        }

        var comment = new Comment
        {
            PasteId = pasteId,
            UserId = userId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        if (request.ParentId.HasValue)
        {
            var parentComment = await db.Comments.FirstOrDefaultAsync(c => c.Id == request.ParentId.Value, cancellationToken);
            if (parentComment == null)
            {
                throw new CommentNotFoundException($"Parent comment with ID '{request.ParentId.Value}' not found.");
            }
            if (parentComment.PasteId != pasteId)
            {
                throw new InvalidOperationException("Parent comment does not belong to the same paste.");
            }
            comment.CommentId = request.ParentId.Value;
        }

        await db.Comments.AddAsync(comment, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created new comment with ID '{CommentId}'", comment.Id);

        return new(comment.Id, comment.Content, comment.CreatedAt, 0, 0, user.Username ?? string.Empty, user.ImageUrl ?? string.Empty);
    }

    public async Task<CommentResponse> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            logger.LogWarning("Attempted to get comment with ID '{CommentId}' that does not exist.", commentId);
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found.");
        }
        
        var response = new CommentResponse(
            comment.Id,
            comment.Content,
            comment.CreatedAt,
            comment.Votes.Count(v => v.IsUpvote),
            comment.Votes.Count(v => !v.IsUpvote),
            comment.User?.Username ?? string.Empty,
            comment.User?.ImageUrl ?? string.Empty);
        
        var replies = await db.Comments
            .Where(c => c.CommentId == commentId)
            .Include(c => c.User)
            .Include(c => c.Votes)
            .ToListAsync(cancellationToken);

        foreach (var reply in replies)
        {
            response.Replies.Add(new CommentResponse(
                reply.Id,
                reply.Content,
                reply.CreatedAt,
                reply.Votes.Count(v => v.IsUpvote),
                reply.Votes.Count(v => !v.IsUpvote),
                reply.User?.Username ?? string.Empty,
                reply.User?.ImageUrl ?? string.Empty));
        }

        return response;
    }

    public async Task<PaginatedResponse<CommentResponse>> GetCommentsAsync(Guid pasteId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var allComments = await db.Comments
            .Where(c => c.PasteId == pasteId)
            .Include(c => c.User)
            .Include(c => c.Votes)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var commentMap = allComments.ToDictionary(
            c => c.Id,
            c => new CommentResponse(
                c.Id,
                c.Content,
                c.CreatedAt,
                c.Votes.Count(v => v.IsUpvote),
                c.Votes.Count(v => !v.IsUpvote),
                c.User?.Username,
                c.User?.ImageUrl
            )
        );

        var topLevelComments = new List<CommentResponse>();

        foreach (var comment in allComments)
        {
            if (comment.CommentId.HasValue && commentMap.TryGetValue(comment.CommentId.Value, out var parentComment))
            {
                parentComment.Replies.Add(commentMap[comment.Id]);
            }
            else
            {
                topLevelComments.Add(commentMap[comment.Id]);
            }
        }
        
        foreach (var comment in commentMap.Values)
        {
            comment.Replies = comment.Replies.OrderBy(r => r.CreatedAt).ToList();
        }

        var paginatedTopLevelComments = topLevelComments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        logger.LogInformation("Retrieved page {PageNumber} of comments for paste ID '{PasteId}' with page size {PageSize}", pageNumber, pasteId, pageSize);

        return new(paginatedTopLevelComments, topLevelComments.Count, pageNumber, pageSize);
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

    public async Task UpdateCommentAsync(Guid commentId, Guid currentUserId, CommentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments.FindAsync([commentId], cancellationToken);

        if (comment == null || comment.UserId != currentUserId)
        {
            logger.LogWarning("Attempted to update comment with ID '{CommentId}' that does not exist or is not owned by the current user.", commentId);
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found or is not owned by the current user.");
        }

        comment.Content = request.Content ?? string.Empty;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated comment with ID '{CommentId}'", comment.Id);
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments.FindAsync([commentId], cancellationToken);
        if (comment == null || comment.UserId != currentUserId)
        {
            logger.LogWarning("Attempted to delete comment with ID '{CommentId}' that does not exist or is not owned by the current user.", commentId);
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found or is not owned by the current user.");
        }

        db.Comments.Remove(comment!);

        if (comment.CommentId.HasValue)
        {
            var parentComment = await db.Comments.FindAsync(comment.CommentId.Value, cancellationToken);
            if (parentComment != null)
            {
                parentComment.Replies.Remove(comment);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
