using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Pastebin.DTOs.Comment.Requests;
using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Extensions;
using Pastebin.Services.Interfaces;

namespace Pastebin.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments").WithTags("Comments").RequireAuthorization();

        group.MapGet("/paste/{pasteId:guid}", GetComments).AllowAnonymous();
        group.MapGet("/user/{userId:guid}", GetCommentsByUserId).AllowAnonymous();
        group.MapPost("/paste/{pasteId:guid}", CreateComment);
        group.MapGet("/{commentId:guid}", GetComment).AllowAnonymous();
        group.MapPut("/{commentId:guid}", UpdateComment);
        group.MapDelete("/{commentId:guid}", DeleteComment);
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
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var response = await commentService.CreateCommentAsync(pasteId, userId.Value, request);
        return TypedResults.Created($"/api/comments/{response.Id}", response);
    }

    private static async Task<Ok<CommentResponse>> GetComment(Guid commentId, ICommentService commentService)
    {
        var response = await commentService.GetCommentAsync(commentId);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> UpdateComment(Guid commentId, ClaimsPrincipal principal, CommentUpdateRequest request, ICommentService commentService)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        await commentService.UpdateCommentAsync(commentId, userId.Value, request);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DeleteComment(Guid commentId, ClaimsPrincipal principal, ICommentService commentService)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        await commentService.DeleteCommentAsync(commentId, userId.Value);
        return TypedResults.NoContent();
    }
}