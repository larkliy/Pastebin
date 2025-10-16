using System.Security.Claims;
using Pastebin.DTOs.Comment.Requests;
using Pastebin.DTOs.Comment.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Services;

namespace Pastebin.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments").WithTags("Comments").RequireAuthorization();

        group.MapGet("/paste/{pasteId}", async (Guid pasteId, int pageNumber, int pageSize, ICommentService commentService) =>
        {
            var response = await commentService.GetCommentsAsync(pasteId, pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetComments")
            .WithSummary("Get comments")
            .WithDescription("Retrieves a list of comments for a given paste ID.")
            .Produces<PaginatedResponse<CommentResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapGet("/user/{userId}", async (Guid userId, int pageNumber, int pageSize, ICommentService commentService) =>
        {
            var response = await commentService.GetCommentsByUserIdAsync(userId, pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetCommentsByUserId")
            .WithSummary("Get comments by user ID")
            .WithDescription("Retrieves a list of comments for a given user ID.")
            .Produces<PaginatedResponse<CommentResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapPost("/paste/{pasteId}", async (Guid pasteId, CommentCreateRequest request, ClaimsPrincipal principal, ICommentService commentService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await commentService.CreateCommentAsync(pasteId, userId, request.Content);
            return Results.Created($"/api/comments/{response.Id}", response);
        })
            .WithName("CreateComment")
            .WithSummary("Create a comment")
            .WithDescription("Creates a new comment with the provided content.")
            .Produces<CommentResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        group.MapGet("/{commentId}", async (Guid commentId, ICommentService commentService) =>
        {
            var response = await commentService.GetCommentAsync(commentId);
            return Results.Ok(response);
        })
            .WithName("GetComment")
            .WithSummary("Get a comment")
            .WithDescription("Retrieves a comment with the provided ID.")
            .Produces<CommentResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapPut("/{commentId}", async (Guid commentId, CommentCreateRequest request, ICommentService commentService) =>
        {
            await commentService.UpdateCommentAsync(commentId, request.Content);
            return Results.NoContent();
        })
            .WithName("UpdateComment")
            .WithSummary("Update a comment")
            .WithDescription("Updates a comment with the provided ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        group.MapDelete("/{commentId}", async (Guid commentId, ICommentService commentService) =>
        {
            await commentService.DeleteCommentAsync(commentId);
            return Results.NoContent();
        })
            .WithName("DeleteComment")
            .WithSummary("Delete a comment")
            .WithDescription("Deletes a comment with the provided ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();
    }
}