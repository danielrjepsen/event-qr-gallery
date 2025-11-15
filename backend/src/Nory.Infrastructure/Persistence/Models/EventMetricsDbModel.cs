using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Nory.Core.Domain.Enums;
using Nory.Infrastructure.Persistence.Models;

public class EventMetricsDbModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EventId { get; set; }

    [Required]
    public MetricsPeriodType PeriodType { get; set; }

    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    public int TotalPhotosUploaded { get; set; } = 0;
    public int TotalGuestAppOpens { get; set; } = 0;
    public int TotalQrScans { get; set; } = 0;
    public int TotalSlideshowViews { get; set; } = 0;
    public int TotalGalleryViews { get; set; } = 0;
    public int LiveGuestCount { get; set; } = 0;

    // extended metrics in JSON format
    public JsonDocument? FeatureUsage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public EventDbModel? Event { get; set; }
}
