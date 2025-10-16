using System.Security.Claims;
using Pastebin.DTOs.Like;
using Pastebin.DTOs.Like.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Services;

namespace Pastebin.Endpoints;

public static class LikeEndpoints
{
    public static void MapLikeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/likes").WithTags("Likes").RequireAuthorization();

        group.MapGet("/my-likes", async (ClaimsPrincipal principal, int pageNumber, int pageSize, ILikeService likeService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await likeService.GetLikesByUserIdAsync(userId, pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetMyLikes")
            .WithSummary("Get the current user's likes")
            .WithDescription("Retrieves a list of likes for the currently authenticated user.")
            .Produces<PaginatedResponse<LikeResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        group.MapGet("/paste/{pasteId}", async (Guid pasteId, int pageNumber, int pageSize, ILikeService likeService) =>
        {
            var response = await likeService.GetLikesByPasteIdAsync(pasteId, pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetLikesByPasteId")
            .WithSummary("Get likes by paste ID")
            .WithDescription("Retrieves a list of likes for a given paste ID.")
            .Produces<PaginatedResponse<LikeResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapPost("/paste", async (ClaimsPrincipal principal, Guid pasteId, ILikeService likeService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await likeService.LikePasteAsync(userId, pasteId);
            return Results.Created($"/api/likes/paste/{response.Id}", response);
        })
            .WithName("LikePaste")
            .WithSummary("Like a paste")
            .WithDescription("Likes a paste with the provided user ID and paste ID.")
            .Produces<LikeCreateResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        group.MapDelete("/paste/{pasteId}", async (ClaimsPrincipal principal, Guid pasteId, ILikeService likeService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            await likeService.DeleteLikeAsync(userId, pasteId);

            return Results.NoContent();
        })
            .WithName("DeleteLike")
            .WithSummary("Delete a like")
            .WithDescription("Deletes a like with the provided user ID and paste ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();
    }
}