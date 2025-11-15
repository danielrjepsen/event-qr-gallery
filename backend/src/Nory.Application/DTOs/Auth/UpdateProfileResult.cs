namespace Nory.Application.DTOs.Auth;

public class UpdateProfileResult
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public List<string> Errors { get; set; } = new();
}