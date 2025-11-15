using Nory.Core.Domain.Enums;

namespace Nory.Application.DTOs;

public class EventMetricsDto
{
    public Guid EventId { get; set; }
    public MetricsPeriodType PeriodType { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    public int TotalPhotosUploaded { get; set; }
    public int TotalGuestAppOpens { get; set; }
    public int TotalQrScans { get; set; }
    public int TotalSlideshowViews { get; set; }
    public int TotalGalleryViews { get; set; }
    public int LiveGuestCount { get; set; }

    public DateTime UpdatedAt { get; set; }
}
