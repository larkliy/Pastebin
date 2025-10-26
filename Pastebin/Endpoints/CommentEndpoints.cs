using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Pastebin.DTOs.Comment.Requests;
using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Services.Interfaces;

namespace Pastebin.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments").WithTags("Comments").RequireAuthorization();

        group.MapGet("/paste/{pasteId}", GetComments).AllowAnonymous();
        group.MapGet("/user/{userId}", GetCommentsByUserId).AllowAnonymous();
        group.MapPost("/paste/{pasteId}", CreateComment);
        group.MapGet("/{commentId}", GetComment).AllowAnonymous();
        group.MapPut("/{commentId}", UpdateComment);
        group.MapDelete("/{commentId}", DeleteComment);
    }

    private static async Task<Ok<PaginatedResponse<CommentResponse>>> GetComments(Guid pasteId, int pageNumber, int pageSize, ICommentService commentService)
    {
        var response = await commentService.GetCommentsAsync(pasteId, pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<PaginatedResponse<CommentResponse>>> GetCommentsByUserId(Guid userId, int pageNumber, int pageSize, ICommentService commentService)
    {
        var response = await commentService.GetCommentsByUserIdAsync(userId, pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Created<CommentResponse>, UnauthorizedHttpResult>> CreateComment(Guid pasteId, CommentCreateRequest request, ClaimsPrincipal principal, ICommentService commentService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var response = await commentService.CreateCommentAsync(pasteId, userId, request);
        return TypedResults.Created($"/api/comments/{response.Id}", response);
    }

    private static async Task<Ok<CommentResponse>> GetComment(Guid commentId, ICommentService commentService)
    {
        var response = await commentService.GetCommentAsync(commentId);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> UpdateComment(Guid commentId, ClaimsPrincipal principal, CommentUpdateRequest request, ICommentService commentService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await commentService.UpdateCommentAsync(commentId, userId, request);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DeleteComment(Guid commentId, ClaimsPrincipal principal, ICommentService commentService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await commentService.DeleteCommentAsync(commentId, userId);
        return TypedResults.NoContent();
    }
}