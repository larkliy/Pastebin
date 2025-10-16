using Pastebin.DTOs.CommentVote.Responses;

namespace Pastebin.Services;

public interface ICommentVoteService
{
    Task<CommentVoteResponse> VoteAsync(Guid commentId, Guid userId, bool isUpvote, CancellationToken cancellationToken = default);
}
