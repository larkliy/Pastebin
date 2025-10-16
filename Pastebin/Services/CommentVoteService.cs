using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.DTOs.CommentVote.Responses;
using Pastebin.Exceptions.Comment;
using Pastebin.Models;

namespace Pastebin.Services;

public class CommentVoteService(AppDbContext db, ILogger<CommentVoteService> logger) : ICommentVoteService
{
    public async Task<CommentVoteResponse> VoteAsync(Guid commentId, Guid userId, bool isUpvote, CancellationToken cancellationToken = default)
    {
        var commentExists = await db.Comments.AnyAsync(c => c.Id == commentId, cancellationToken);
        if (!commentExists)
        {
            throw new CommentNotFoundException($"Comment with ID '{commentId}' not found.");
        }

        var existingVote = await db.CommentVotes
            .FirstOrDefaultAsync(v => v.CommentId == commentId && v.UserId == userId, cancellationToken);

        if (existingVote != null)
        {
            if (existingVote.IsUpvote == isUpvote)
            {
                db.CommentVotes.Remove(existingVote);
                logger.LogInformation("User '{UserId}' removed their vote from comment '{CommentId}'", userId, commentId);
            }
            else
            {
                existingVote.IsUpvote = isUpvote;
                db.CommentVotes.Update(existingVote);
                logger.LogInformation("User '{UserId}' changed their vote on comment '{CommentId}' to '{IsUpvote}'", userId, commentId, isUpvote);
            }
        }
        else
        {
            var newVote = new CommentVote
            {
                CommentId = commentId,
                UserId = userId,
                IsUpvote = isUpvote
            };
            await db.CommentVotes.AddAsync(newVote, cancellationToken);
            logger.LogInformation("User '{UserId}' voted on comment '{CommentId}' with '{IsUpvote}'", userId, commentId, isUpvote);
        }

        await db.SaveChangesAsync(cancellationToken);

        var upvotes = await db.CommentVotes.CountAsync(v => v.CommentId == commentId && v.IsUpvote, cancellationToken);
        var downvotes = await db.CommentVotes.CountAsync(v => v.CommentId == commentId && !v.IsUpvote, cancellationToken);

        return new(upvotes, downvotes);
    }
}
