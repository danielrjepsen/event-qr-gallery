using System.Text.Json;
using Nory.Core.Domain.Enums;

namespace Nory.Core.Domain.Entities;

public class EventMetrics
{
    public Guid Id { get; internal set; }
    public Guid EventId { get; internal set; }
    public MetricsPeriodType PeriodType { get; internal set; }
    public DateTime? PeriodStart { get; internal set; }
    public DateTime? PeriodEnd { get; internal set; }

    public int TotalPhotosUploaded { get; internal set; }
    public int TotalGuestAppOpens { get; internal set; }
    public int TotalQrScans { get; internal set; }
    public int TotalSlideshowViews { get; internal set; }
    public int TotalGalleryViews { get; internal set; }
    public int LiveGuestCount { get; internal set; }

    public JsonDocument? FeatureUsage { get; internal set; }

    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }

    public EventMetrics(
        Guid eventId,
        MetricsPeriodType periodType,
        DateTime? periodStart = null,
        DateTime? periodEnd = null
    )
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId is required", nameof(eventId));

        Id = Guid.NewGuid();
        EventId = eventId;
        PeriodType = periodType;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;

        TotalPhotosUploaded = 0;
        TotalGuestAppOpens = 0;
        TotalQrScans = 0;
        TotalSlideshowViews = 0;
        TotalGalleryViews = 0;
        LiveGuestCount = 0;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Business methods
    public void IncrementPhotoUploads(int count = 1)
    {
        TotalPhotosUploaded += count;
        Touch();
    }

    public void IncrementGuestAppOpens(int count = 1)
    {
        TotalGuestAppOpens += count;
        Touch();
    }

    public void IncrementQrScans(int count = 1)
    {
        TotalQrScans += count;
        Touch();
    }

    public void IncrementSlideshowViews(int count = 1)
    {
        TotalSlideshowViews += count;
        Touch();
    }

    public void IncrementGalleryViews(int count = 1)
    {
        TotalGalleryViews += count;
        Touch();
    }

    public void UpdateLiveGuestCount(int count)
    {
        LiveGuestCount = Math.Max(0, count);
        Touch();
    }

    public void UpdateFeatureUsage(JsonDocument featureUsage)
    {
        FeatureUsage = featureUsage;
        Touch();
    }

    public void ResetMetrics()
    {
        TotalPhotosUploaded = 0;
        TotalGuestAppOpens = 0;
        TotalQrScans = 0;
        TotalSlideshowViews = 0;
        TotalGalleryViews = 0;
        LiveGuestCount = 0;
        FeatureUsage = null;
        Touch();
    }

    // Business query methods
    public bool HasActivity()
    {
        return TotalPhotosUploaded > 0
            || TotalGuestAppOpens > 0
            || TotalQrScans > 0
            || TotalSlideshowViews > 0
            || TotalGalleryViews > 0;
    }

    public bool IsTotalMetrics() => PeriodType == MetricsPeriodType.Total;

    public bool IsHourlyMetrics() => PeriodType == MetricsPeriodType.Hourly;

    public bool IsDailyMetrics() => PeriodType == MetricsPeriodType.Daily;

    public bool IsWeeklyMetrics() => PeriodType == MetricsPeriodType.Weekly;

    public bool IsMonthlyMetrics() => PeriodType == MetricsPeriodType.Monthly;

    public bool IsTimePeriodMetrics() => PeriodType != MetricsPeriodType.Total;

    public int GetTotalEngagement()
    {
        return TotalPhotosUploaded
            + TotalGuestAppOpens
            + TotalQrScans
            + TotalSlideshowViews
            + TotalGalleryViews;
    }

    // Helper methods for feature JsonDocument
    public T? GetFeatureValue<T>(string key)
    {
        if (FeatureUsage?.RootElement.TryGetProperty(key, out var value) == true)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value.GetRawText());
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    public bool HasFeature(string key)
    {
        return FeatureUsage?.RootElement.TryGetProperty(key, out _) == true;
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
