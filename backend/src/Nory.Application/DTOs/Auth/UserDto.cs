namespace Nory.Application.DTOs.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; }
}