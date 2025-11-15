namespace Nory.Application.DTOs.Auth;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public List<string> Errors { get; set; } = new();
}