using Pastebin.DTOs.Shared;
using Pastebin.DTOs.User.Requests;
using Pastebin.DTOs.User.Responses;

namespace Pastebin.Services;

public interface IUserService
{
    Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<UserCreateResponse> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default);
    Task<UserLoginResponse> AuthenticateUserAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<UserLoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<FoundUserResponseUser>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateUserByIdAsync(Guid userId, UpdateUserRequest updateRequest, CancellationToken cancellationToken = default);
    Task DeleteUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
