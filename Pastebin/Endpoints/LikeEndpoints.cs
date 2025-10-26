using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Pastebin.DTOs.Like.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Extensions;
using Pastebin.Services.Interfaces;

namespace Pastebin.Endpoints;

public static class LikeEndpoints
{
    public static void MapLikeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/likes").WithTags("Likes").RequireAuthorization();

        group.MapGet("/my-likes", GetMyLikes);
        group.MapGet("/paste/{pasteId:guid}", GetLikesByPasteId).AllowAnonymous();
        group.MapPost("/paste", LikePaste);
        group.MapDelete("/paste/{pasteId:guid}", DeleteLike);
    }

    private static async Task<Results<Ok<PaginatedResponse<LikeResponse>>, UnauthorizedHttpResult>> GetMyLikes(ClaimsPrincipal principal, int pageNumber, int pageSize, ILikeService likeService)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var response = await likeService.GetLikesByUserIdAsync(userId.Value, pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<PaginatedResponse<LikeResponse>>> GetLikesByPasteId(Guid pasteId, int pageNumber, int pageSize, ILikeService likeService)
    {
        var response = await likeService.GetLikesByPasteIdAsync(pasteId, pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Created<LikeCreateResponse>, UnauthorizedHttpResult>> LikePaste(ClaimsPrincipal principal, Guid pasteId, ILikeService likeService)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var response = await likeService.LikePasteAsync(userId.Value, pasteId);
        return TypedResults.Created($"/api/likes/paste/{response.Id}", response);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DeleteLike(ClaimsPrincipal principal, Guid pasteId, ILikeService likeService)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        await likeService.DeleteLikeAsync(userId.Value, pasteId);

        return TypedResults.NoContent();
    }
}