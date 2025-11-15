using Nory.Application.DTOs.Auth;

namespace Nory.Application.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> VerifyEmailAsync(string userId, string token);
    Task SendPasswordResetEmailAsync(string email);
    Task<bool> ResetPasswordAsync(string userId, string token, string newPassword);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<UserDto?> GetCurrentUserAsync(string userId);
    Task<UpdateProfileResult> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task LogoutAsync(string userId);
}