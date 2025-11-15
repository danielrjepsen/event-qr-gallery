using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Nory.Application.DTOs.Auth;
using Nory.Core.Entities;
using Nory.Core.Domain.Entities;

namespace Nory.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { "Email already registered" }
            };
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return new AuthResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        // Generate email confirmation token
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // TODO: Send email with verification link

        var (token, refreshToken) = await GenerateTokensAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { "Invalid email or password" }
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            var errors = new List<string>();
            
            if (result.IsLockedOut)
                errors.Add("Account is locked out");
            else if (result.IsNotAllowed)
                errors.Add("Account is not allowed to sign in");
            else
                errors.Add("Invalid email or password");

            return new AuthResult
            {
                Success = false,
                Errors = errors
            };
        }

        var (token, refreshToken) = await GenerateTokensAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implement refresh token validation from database
        // For now, just decode and regenerate
        
        var principal = GetPrincipalFromExpiredToken(refreshToken);
        if (principal == null)
        {
            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { "Invalid refresh token" }
            };
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { "Invalid refresh token" }
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { "User not found" }
            };
        }

        var (newToken, newRefreshToken) = await GenerateTokensAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = newToken,
            RefreshToken = newRefreshToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<bool> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return; // Don't reveal if user exists

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // TODO: Send email with reset link containing token
        _logger.LogInformation("Password reset token generated for {Email}", email);
    }

    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        return MapToUserDto(user);
    }

    public async Task<UpdateProfileResult> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new UpdateProfileResult
            {
                Success = false,
                Errors = new List<string> { "User not found" }
            };
        }

        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrEmpty(request.ProfilePicture))
            user.ProfilePicture = request.ProfilePicture;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new UpdateProfileResult
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        return new UpdateProfileResult
        {
            Success = true,
            User = MapToUserDto(user)
        };
    }

    public async Task LogoutAsync(string userId)
    {
        // TODO: Invalidate refresh tokens in database
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserId} logged out", userId);
    }

    #region Private Helper Methods

    private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.Name ?? user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var refreshToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            new JwtSecurityTokenHandler().WriteToken(refreshToken)
        );
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured"))),
            ValidateLifetime = false // Don't validate expiration for refresh
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.Name ?? user.Email!,
            EmailVerified = user.EmailConfirmed,
            ProfilePicture = user.ProfilePicture,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion
}