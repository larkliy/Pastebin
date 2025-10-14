using Pastebin.DTOs.Shared;
using Pastebin.DTOs.User.Requests;
using Pastebin.DTOs.User.Responses;
using Pastebin.Services;
using System.Security.Claims;

namespace Pastebin.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/register", async (UserCreateRequest request, IUserService userService) =>
        {
            var response = await userService.CreateUserAsync(request.Username, request.Email, request.Password);
            return Results.Created($"/api/users/{response.Id}", response);
        })
            .WithName("RegisterUser")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with the provided username, email, and password.")
            .Produces<UserCreateResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapPost("/login", async (UserLoginRequest request, IUserService userService) =>
        {
            var response = await userService.AuthenticateUserAsync(request.Username, request.Password);
            return Results.Ok(response);
        })
            .WithName("LoginUser")
            .WithSummary("Authenticate a user")
            .WithDescription("Authenticates a user with the provided username and password, returning JWT tokens upon success.")
            .Produces<UserLoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapPost("/refresh-token", async (string refreshToken, IUserService userService) =>
        {
            var response = await userService.RefreshTokenAsync(refreshToken);
            return Results.Ok(response);
        })
            .WithName("RefreshToken")
            .WithSummary("Refresh JWT tokens")
            .WithDescription("Refreshes JWT tokens using a valid refresh token.")
            .Produces<UserLoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .AllowAnonymous();

        group.MapGet("/", async (int pageNumber, int pageSize, IUserService userService) =>
        {
            var response = await userService.GetUsersAsync(pageNumber, pageSize);
            return Results.Ok(response);
        })
            .WithName("GetUsers")
            .WithSummary("Get paginated list of users")
            .WithDescription("Retrieves a paginated list of users.")
            .Produces<PaginatedResponse<FoundedUserResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .RequireAuthorization();

        group.MapDelete("/me", async (ClaimsPrincipal principal, IUserService userService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            await userService.DeleteUserByIdAsync(userId, userId);
            return Results.NoContent();
        })
            .WithName("DeleteCurrentUser")
            .WithSummary("Deletes the currently authenticated user's account.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .RequireAuthorization();

        group.MapPut("/me", async (ClaimsPrincipal principal, UpdateUserRequest request, IUserService userService) =>
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Results.Unauthorized();
            }

            var response = await userService.UpdateUserByIdAsync(userId, userId, request);
            return Results.Ok(response);
        })
            .WithSummary("Update the currently authenticated user's information.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .RequireAuthorization();
    }
}
