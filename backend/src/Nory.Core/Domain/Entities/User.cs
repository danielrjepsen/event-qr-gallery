// Core/Domain/Entities/User.cs
using Microsoft.AspNetCore.Identity;

namespace Nory.Core.Domain.Entities;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string Locale { get; set; } = "en";
    public string? ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}