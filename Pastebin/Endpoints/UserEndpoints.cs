using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Pastebin.DTOs.Shared;
using Pastebin.DTOs.User.Requests;
using Pastebin.DTOs.User.Responses;
using Pastebin.Services.Interfaces;

namespace Pastebin.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/register", RegisterUser).AllowAnonymous();
        group.MapPost("/login", LoginUser).AllowAnonymous();
        group.MapPost("/refresh-token", RefreshToken).AllowAnonymous();
        group.MapGet("/", GetUsers).RequireAuthorization();
        group.MapDelete("/me", DeleteCurrentUser).RequireAuthorization();
        group.MapPut("/me", UpdateCurrentUser).RequireAuthorization();
    }

    private static async Task<Created<UserCreateResponse>> RegisterUser(UserCreateRequest request, IUserService userService)
    {
        var response = await userService.CreateUserAsync(request.Username, request.Email, request.Password);
        return TypedResults.Created($"/api/users/{response.Id}", response);
    }

    private static async Task<Ok<UserLoginResponse>> LoginUser(UserLoginRequest request, IUserService userService)
    {
        var response = await userService.AuthenticateUserAsync(request.Username, request.Password);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<UserLoginResponse>> RefreshToken(string refreshToken, IUserService userService)
    {
        var response = await userService.RefreshTokenAsync(refreshToken);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<PaginatedResponse<FoundUserResponseUser>>> GetUsers(int pageNumber, int pageSize, IUserService userService)
    {
        var response = await userService.GetUsersAsync(pageNumber, pageSize);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> DeleteCurrentUser(ClaimsPrincipal principal, IUserService userService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await userService.DeleteUserByIdAsync(userId);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<UserResponse>, UnauthorizedHttpResult>> UpdateCurrentUser(ClaimsPrincipal principal, UpdateUserRequest request, IUserService userService)
    {
        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var response = await userService.UpdateUserByIdAsync(userId, request);
        return TypedResults.Ok(response);
    }
}
