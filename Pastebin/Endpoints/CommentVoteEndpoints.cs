using System.Security.Claims;
using Pastebin.DTOs.CommentVote.Requests;
using Pastebin.DTOs.CommentVote.Responses;
using Pastebin.Services;

namespace Pastebin.Endpoints;

public static class CommentVoteEndpoints
{
    public static void MapCommentVoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments/{commentId:guid}/vote")
            .WithTags("Comment Votes")
            .RequireAuthorization();

        group.MapPost("/", async (Guid commentId, CommentVoteRequest request, ClaimsPrincipal principal, ICommentVoteService voteService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await voteService.VoteAsync(commentId, userId, request.IsUpvote);
            return Results.Ok(response);
        })
        .WithName("VoteOnComment")
        .WithSummary("Vote on a comment")
        .WithDescription("Casts, updates, or removes a vote on a specific comment. If the same vote is sent twice, it's removed.")
        .Produces<CommentVoteResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesValidationProblem();
    }
}
