using Microsoft.EntityFrameworkCore;
using Pastebin.Application;
using Pastebin.DTOs.Shared;
using Pastebin.DTOs.User.Requests;
using Pastebin.DTOs.User.Responses;
using Pastebin.Exceptions.User;
using Pastebin.Models;

namespace Pastebin.Services;

public class UserService(AppDbContext db, IJwtService jwtService, ILogger<UserService> logger) : IUserService
{
    public async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if user with username '{Username}' exists.", username);
        return await db.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Checking if user with email '{Email}' exists.", email);
        return await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<UserCreateResponse> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default)
    {
        if (await UserExistsAsync(username, cancellationToken))
        {
            logger.LogWarning("Attempted to create user with existing username '{Username}'", username);
            throw new UserAlreadyExistsException($"Username '{username}' is already taken.");
        }

        if (await EmailExistsAsync(email, cancellationToken))
        {
            logger.LogWarning("Attempted to create user with existing email '{Email}'", email);
            throw new UserAlreadyExistsException($"Email '{email}' is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created new user with ID '{UserId}'", user.Id);

        return new(user.Id, user.Username, user.Email, user.CreatedAt);
    }

    public async Task<UserLoginResponse> AuthenticateUserAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for username '{Username}'", username);
            throw new InvalidCredentialsException("Invalid username or password.");
        }

        var accessToken = await jwtService.GenerateTokenAsync(user.Id, user.Username);
        var refreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User '{Username}' authenticated successfully.", username);

        return new(user.Id, user.Username, user.Email, user.CreatedAt, accessToken, refreshToken);
    }

    public async Task<UserLoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            logger.LogWarning("Invalid or expired refresh token used.");
            throw new InvalidRefreshTokenException("Invalid or expired refresh token.");
        }

        var newAccessToken = await jwtService.GenerateTokenAsync(user.Id, user.Username);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await db.SaveChangesAsync(cancellationToken);

        return new(user.Id, user.Username, user.Email, user.CreatedAt, newAccessToken, newRefreshToken);
    }

    public async Task<PaginatedResponse<FoundedUserResponse>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var totalUsers = await db.Users.CountAsync(cancellationToken);

        var users = await db.Users
            .OrderBy(u => u.Username)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new FoundedUserResponse(u.Id, u.Username, u.CreatedAt))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved page {PageNumber} of users with page size {PageSize}", pageNumber, pageSize);

        return new(users, totalUsers, pageNumber, pageSize);
    }

    public async Task<UserResponse> UpdateUserByIdAsync(Guid userId, Guid currentUserId, UpdateUserRequest updateRequest, CancellationToken cancellationToken = default)
    {
        if (userId != currentUserId)
        {
            logger.LogWarning("Attempted to update user with ID '{UserId}' by user with ID '{CurrentUserId}'", userId, currentUserId);
            throw new UnauthorizedAccessException("You are not authorized to update this user.");
        }
        
        var user = await db.Users.FindAsync([userId], cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Attempted to update non-existent user with ID '{UserId}'", userId);
            throw new UserNotFoundException($"User with ID '{userId}' not found.");
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Username) && updateRequest.Username != user.Username)
        {
            if (await UserExistsAsync(updateRequest.Username, cancellationToken))
            {
                logger.LogWarning("Attempted to update user ID '{UserId}' to existing username '{Username}'", userId, updateRequest.Username);
                throw new UserAlreadyExistsException($"Username '{updateRequest.Username}' is already taken.");
            }
            user.Username = updateRequest.Username;
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Email) && updateRequest.Email != user.Email)
        {
            if (await EmailExistsAsync(updateRequest.Email, cancellationToken))
            {
                logger.LogWarning("Attempted to update user ID '{UserId}' to existing email '{Email}'", userId, updateRequest.Email);
                throw new UserAlreadyExistsException($"Email '{updateRequest.Email}' is already registered.");
            }
            user.Email = updateRequest.Email;
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateRequest.Password);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated user with ID '{UserId}'", userId);

        return new(user.Id, user.Username, user.Email, user.CreatedAt);
    }

    public async Task DeleteUserByIdAsync(Guid userId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId != currentUserId)
        {
            logger.LogWarning("User '{CurrentUserId}' attempted to delete user '{UserId}' owned by another user '{OwnerId}'.", currentUserId, userId, currentUserId);
            throw new UserNotFoundException($"User with ID '{userId}' not found.");
        }

        var user = await db.Users.FindAsync([userId], cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Attempted to delete non-existent user with ID '{UserId}'", userId);
            throw new UserNotFoundException($"User with ID '{userId}' not found.");
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted user with ID '{UserId}'", userId);
    }
}
