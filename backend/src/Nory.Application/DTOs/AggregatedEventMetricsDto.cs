namespace Nory.Application.DTOs;

public class AggregatedEventMetricsDto
{
    // totals
    public int TotalPhotosUploaded { get; set; }
    public int TotalGuestAppOpens { get; set; }
    public int TotalQrScans { get; set; }
    public int TotalSlideshowViews { get; set; }
    public int TotalGalleryViews { get; set; }
    public int LiveGuestCount { get; set; }

    // summary
    public int ActiveEvents { get; set; }
    public int TotalEvents { get; set; }
    public int TotalEngagement { get; set; }

    // calculated
    public double PhotosPerEvent => TotalEvents > 0 ? (double)TotalPhotosUploaded / TotalEvents : 0;
}
