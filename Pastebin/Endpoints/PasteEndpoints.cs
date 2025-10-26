
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pastebin.DTOs.Paste.Requests;
using Pastebin.DTOs.Paste.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Services.Interfaces;

namespace Pastebin.Endpoints;

public static class PasteEndpoints
{
    public static void MapPasteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pastes").WithTags("Pastes").RequireAuthorization();

        group.MapGet("/{id:guid}", GetPasteDetails).AllowAnonymous();
        group.MapGet("/my-pastes", GetMyPastes);
        group.MapGet("/", GetPastes).AllowAnonymous();
        group.MapPost("/", CreatePaste).AllowAnonymous();
        group.MapPut("/{id:guid}", UpdatePaste);
        group.MapDelete("/{id:guid}", DeletePaste);
    }

    private static async Task<Ok<PasteDetailsResponse>> GetPasteDetails(Guid id, [FromHeader(Name = "X-Password")] string? password, ClaimsPrincipal principal, IPasteService pasteService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdString, out var userId);

        var response = await pasteService.GetPasteDetailsAsync(id, userId == Guid.Empty ? null : userId, password);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<PaginatedResponse<PasteResponse>>, UnauthorizedHttpResult>> GetMyPastes(ClaimsPrincipal principal, int pageNumber, int pageSize, IPasteService pasteService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var response = await pasteService.GetPastesByUserIdAsync(userId, pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<PaginatedResponse<PasteResponse>>> GetPastes(int pageNumber, int pageSize, IPasteService pasteService)
    {
        var response = await pasteService.GetPastesAsync(pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Created<PasteCreateResponse>> CreatePaste(PasteCreateRequest request, ClaimsPrincipal principal, IPasteService pasteService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdString, out var userId);

        var response = await pasteService.CreatePasteAsync(userId == Guid.Empty ? null : userId, request);
        return TypedResults.Created($"/api/pastes/{response.Id}", response);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> UpdatePaste(Guid id, PasteUpdateRequest request, ClaimsPrincipal principal, IPasteService pasteService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await pasteService.UpdatePasteAsync(id, userId, request);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DeletePaste(Guid id, ClaimsPrincipal principal, IPasteService pasteService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await pasteService.DeletePasteAsync(id, userId);
        return TypedResults.NoContent();
    }
}