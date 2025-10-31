using Microsoft.EntityFrameworkCore;
using Pastebin.Infrastructure;
using Pastebin.DTOs.Shared;
using Pastebin.DTOs.User.Requests;
using Pastebin.DTOs.User.Responses;
using Pastebin.Exceptions.User;
using Pastebin.Models;
using Pastebin.Services.Interfaces;
using Microsoft.Extensions.Options;
using Pastebin.ConfigurationSettings;

namespace Pastebin.Services.Implementations;

public class UserService(
    AppDbContext db,
    IJwtService jwtService,
    IEmailService emailService,
    IOptions<ApplicationSettings> applicationSettings,
    ILogger<UserService> logger) : IUserService
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
            CreatedAt = DateTime.UtcNow,

            EmailConfirmationToken = Guid.NewGuid().ToString(),
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created new user with ID '{UserId}'", user.Id);
        

        var confirmationLink = $"{applicationSettings.Value.FrontendUrl}/api/users/confirm-email?email={user.Email}&token={user.EmailConfirmationToken}";

        var message = $"<p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";
        await emailService.SendEmailAsync(user.Email, "Confirm your email", message, cancellationToken);

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

        var accessToken = await jwtService.GenerateTokenAsync(user);
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

        var newAccessToken = await jwtService.GenerateTokenAsync(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await db.SaveChangesAsync(cancellationToken);

        return new(user.Id, user.Username, user.Email, user.CreatedAt, newAccessToken, newRefreshToken);
    }

    public async Task<PaginatedResponse<FoundUserResponseUser>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var usersQuery = db.Users
            .OrderBy(u => u.Username);

        var users = await usersQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize + 1)
            .Select(u => new FoundUserResponseUser(u.Id, u.Username, u.CreatedAt))
            .ToListAsync(cancellationToken);

        var hasNextPage = users.Count > pageSize;
        var items = hasNextPage ? users.Take(pageSize) : users;

        logger.LogInformation("Retrieved page {PageNumber} of users with page size {PageSize}", pageNumber, pageSize);

        return new(items, pageNumber, pageSize, hasNextPage);
    }

    public async Task<UserResponse> UpdateUserByIdAsync(Guid userId, UpdateUserRequest updateRequest, CancellationToken cancellationToken = default)
    {
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

        if (!string.IsNullOrWhiteSpace(updateRequest.ImageUrl) && updateRequest.ImageUrl != user.ImageUrl)
        {
            user.ImageUrl = updateRequest.ImageUrl;
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateRequest.Password);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated user with ID '{UserId}'", userId);

        return new(user.Id, user.Username, user.Email, user.CreatedAt);
    }

    public async Task DeleteUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
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

    public async Task<EmailConfirmationResponse> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null || user.EmailConfirmationToken != token || user.EmailConfirmationTokenExpiresAt <= DateTime.UtcNow)
        {
            logger.LogWarning("Invalid email confirmation attempt for email '{Email}'", email);
            throw new InvalidEmailConfirmationTokenException("Invalid email confirmation token.");
        }

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiresAt = null;

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Confirmed email for user with email '{Email}'", email);

        return new(token, user.EmailConfirmationTokenExpiresAt);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Attempted to logout non-existent user with ID '{UserId}'", userId);
            throw new UserNotFoundException($"User with ID '{userId}' not found.");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await db.SaveChangesAsync(cancellationToken);
    }
}
