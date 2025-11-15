using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Nory.Core.Domain.Enums;

namespace Nory.Infrastructure.Persistence.Models;

public class ActivityLogDbModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EventId { get; set; }

    [Required]
    public ActivityType Type { get; set; }

    public JsonDocument? Data { get; set; }

    [MaxLength(100)]
    public string? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsProcessed { get; set; } = false;

    public EventDbModel? Event { get; set; }
}
