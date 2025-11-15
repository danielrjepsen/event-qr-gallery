using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nory.Application.DTOs.Auth;
using Nory.Application.Services;

namespace Nory.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    private const int RefreshTokenExpiryDays = 7;
    private const int AccessTokenExpirySeconds = 3600;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// register user with email and password
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(
                new
                {
                    token = result.Token,
                    user = result.User,
                    expiresIn = AccessTokenExpirySeconds,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email {Email}", request.Email);
            return StatusCode(500, new { error = "Registration failed" });
        }
    }

    /// <summary>
    /// login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(
                new
                {
                    token = result.Token,
                    user = result.User,
                    expiresIn = AccessTokenExpirySeconds,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email {Email}", request.Email);
            return StatusCode(500, new { error = "Login failed" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { error = "Refresh token not found" });
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.Success)
            {
                ClearRefreshTokenCookie();
                return Unauthorized(new { errors = result.Errors });
            }

            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(
                new
                {
                    token = result.Token,
                    user = result.User,
                    expiresIn = AccessTokenExpirySeconds,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, new { error = "Token refresh failed" });
        }
    }

    /// <summary>
    /// Verify email address with token
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(request.UserId, request.Token);

            if (!result)
            {
                return BadRequest(new { error = "Invalid or expired verification token" });
            }

            return Ok(new { message = "Email verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email verification failed for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "Email verification failed" });
        }
    }

    /// <summary>
    /// Request password email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _authService.SendPasswordResetEmailAsync(request.Email);

            // Always return success to prevent email enumeration
            return Ok(
                new { message = "If that email exists, a password reset link has been sent" }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset request failed");
            return StatusCode(500, new { error = "Failed to process password reset" });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(
                request.UserId,
                request.Token,
                request.NewPassword
            );

            if (!result)
            {
                return BadRequest(new { error = "Invalid or expired reset token" });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password reset failed");
            return StatusCode(500, new { error = "Password reset failed" });
        }
    }

    /// <summary>
    /// get current auth user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _authService.GetCurrentUserAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user");
            return StatusCode(500, new { error = "Failed to get user" });
        }
    }

    /// <summary>
    ///  update user profile
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _authService.UpdateProfileAsync(userId, request);

            if (!result.Success)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profile update failed");
            return StatusCode(500, new { error = "Profile update failed" });
        }
    }

    /// <summary>
    /// Change password for loggedin user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(
                userId,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!result)
            {
                return BadRequest(new { error = "Current password is incorrect" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password change failed");
            return StatusCode(500, new { error = "Password change failed" });
        }
    }

    /// <summary>
    /// logout user and invalidate refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                await _authService.LogoutAsync(userId);
            }

            ClearRefreshTokenCookie();

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, new { error = "Logout failed" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
            Path = "/",
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            "refreshToken",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            }
        );
    }
}
