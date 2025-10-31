using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Pastebin.DTOs.CommentVote.Requests;
using Pastebin.DTOs.CommentVote.Responses;
using Pastebin.Extensions;
using Pastebin.Services.Interfaces;

namespace Pastebin.Endpoints;

public static class CommentVoteEndpoints
{
    public static void MapCommentVoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments/vote")
            .WithTags("Comment Votes")
            .RequireAuthorization("EmailConfirmed");

        group.MapPost("/", VoteOnComment);
    }

    private static async Task<Results<Ok<CommentVoteResponse>, UnauthorizedHttpResult>> VoteOnComment(CommentVoteRequest request, ClaimsPrincipal principal, ICommentVoteService voteService)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var response = await voteService.VoteAsync(request.CommentId, userId.Value, request.IsUpvote);
        return TypedResults.Ok(response);
    }
}
