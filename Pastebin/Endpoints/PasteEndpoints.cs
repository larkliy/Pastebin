
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Pastebin.DTOs.Paste.Requests;
using Pastebin.DTOs.Paste.Responses;
using Pastebin.DTOs.Shared;
using Pastebin.Services;

namespace Pastebin.Endpoints;

public static class PasteEndpoints
{
    public static void MapPasteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pastes").WithTags("Pastes").RequireAuthorization();

        group.MapPost("/", async (PasteCreateRequest request, ClaimsPrincipal principal, IPasteService pasteService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out var userId);

            var response = await pasteService.CreatePasteAsync(userId == Guid.Empty ? null : userId, request);
            return Results.Created($"/api/pastes/{response.Id}", response);
        })
            .WithName("CreatePaste")
            .WithSummary("Create a new paste")
            .WithDescription("Creates a new paste with the provided title, content, language, and expiration date.")
            .Produces<PasteCreateResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapGet("/my-pastes", async (ClaimsPrincipal principal, int pageNumber, int pageSize, IPasteService pasteService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await pasteService.GetPastesByUserIdAsync(userId, pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetMyPastes")
            .WithSummary("Get the current user's pastes")
            .WithDescription("Retrieves a list of pastes for the currently authenticated user.")
            .Produces<PaginatedResponse<PasteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/", async (int pageNumber, int pageSize, IPasteService pasteService) =>
        {
            var response = await pasteService.GetPastesAsync(pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetPastes")
            .WithSummary("Get all pastes")
            .WithDescription("Retrieves a list of all pastes.")
            .Produces<PaginatedResponse<PasteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapPut("/{id:guid}", async (Guid id, PasteUpdateRequest request, ClaimsPrincipal principal, IPasteService pasteService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            await pasteService.UpdatePasteAsync(id, userId, request);
            return Results.NoContent();
        })
            .WithName("UpdatePaste")
            .WithSummary("Update a paste")
            .WithDescription("Updates a paste with the provided title, content, or privacy settings.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal principal, IPasteService pasteService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            await pasteService.DeletePasteAsync(id, userId);
            return Results.NoContent();
        })
            .WithName("DeletePaste")
            .WithSummary("Delete a paste")
            .WithDescription("Deletes a specific paste.");

        group.MapGet("/{id:guid}", async (Guid id, [FromHeader(Name = "X-Password")] string? password, ClaimsPrincipal principal, IPasteService pasteService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out var userId);

            var response = await pasteService.GetPasteDetailsAsync(id, userId == Guid.Empty ? null : userId, password);
            return Results.Ok(response);
        })
            .WithName("GetPasteDetails")
            .WithSummary("Get a single paste by ID")
            .WithDescription("Retrieves a paste. For private pastes, requires ownership or the correct password passed in the 'X-Password' header.")
            .Produces<PasteDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }
}