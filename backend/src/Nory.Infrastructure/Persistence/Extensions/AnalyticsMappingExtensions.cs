using System.Text.Json;
using Nory.Core.Domain.Entities;
using Nory.Infrastructure.Persistence.Models;

namespace Nory.Infrastructure.Persistence.Extensions;

public static class AnalyticsMappingExtensions
{
    // ActivityLog mappings
    public static ActivityLog MapToDomain(this ActivityLogDbModel dbModel)
    {
        var activity = new ActivityLog(
            dbModel.EventId,
            dbModel.Type,
            dbModel.Data,
            dbModel.SessionId
        );

        activity.Id = dbModel.Id;
        activity.CreatedAt = dbModel.CreatedAt;
        activity.IsProcessed = dbModel.IsProcessed;

        return activity;
    }

    public static ActivityLogDbModel MapToDbModel(this ActivityLog domainActivity)
    {
        return new ActivityLogDbModel
        {
            Id = domainActivity.Id,
            EventId = domainActivity.EventId,
            Type = domainActivity.Type,
            Data = domainActivity.Data,
            SessionId = domainActivity.SessionId,
            CreatedAt = domainActivity.CreatedAt,
            IsProcessed = domainActivity.IsProcessed,
        };
    }

    // EventMetrics mappings
    public static EventMetrics MapToDomain(this EventMetricsDbModel dbModel)
    {
        var metrics = new EventMetrics(
            dbModel.EventId,
            dbModel.PeriodType,
            dbModel.PeriodStart,
            dbModel.PeriodEnd
        );

        metrics.Id = dbModel.Id;
        metrics.TotalPhotosUploaded = dbModel.TotalPhotosUploaded;
        metrics.TotalGuestAppOpens = dbModel.TotalGuestAppOpens;
        metrics.TotalQrScans = dbModel.TotalQrScans;
        metrics.TotalSlideshowViews = dbModel.TotalSlideshowViews;
        metrics.TotalGalleryViews = dbModel.TotalGalleryViews;
        metrics.LiveGuestCount = dbModel.LiveGuestCount;
        metrics.FeatureUsage = dbModel.FeatureUsage;
        metrics.CreatedAt = dbModel.CreatedAt;
        metrics.UpdatedAt = dbModel.UpdatedAt;

        return metrics;
    }

    public static EventMetricsDbModel MapToDbModel(this EventMetrics domainMetrics)
    {
        return new EventMetricsDbModel
        {
            Id = domainMetrics.Id,
            EventId = domainMetrics.EventId,
            PeriodType = domainMetrics.PeriodType,
            PeriodStart = domainMetrics.PeriodStart,
            PeriodEnd = domainMetrics.PeriodEnd,
            TotalPhotosUploaded = domainMetrics.TotalPhotosUploaded,
            TotalGuestAppOpens = domainMetrics.TotalGuestAppOpens,
            TotalQrScans = domainMetrics.TotalQrScans,
            TotalSlideshowViews = domainMetrics.TotalSlideshowViews,
            TotalGalleryViews = domainMetrics.TotalGalleryViews,
            LiveGuestCount = domainMetrics.LiveGuestCount,
            FeatureUsage = domainMetrics.FeatureUsage,
            CreatedAt = domainMetrics.CreatedAt,
            UpdatedAt = domainMetrics.UpdatedAt,
        };
    }

    // Batch mappings for ActivityLog
    public static List<ActivityLog> MapToDomain(this IEnumerable<ActivityLogDbModel> dbModels)
    {
        return dbModels.Select(db => db.MapToDomain()).ToList();
    }

    public static List<ActivityLogDbModel> MapToDbModel(
        this IEnumerable<ActivityLog> domainActivities
    )
    {
        return domainActivities.Select(a => a.MapToDbModel()).ToList();
    }

    // Batch mappings for EventMetrics
    public static List<EventMetrics> MapToDomain(this IEnumerable<EventMetricsDbModel> dbModels)
    {
        return dbModels.Select(db => db.MapToDomain()).ToList();
    }

    public static List<EventMetricsDbModel> MapToDbModel(
        this IEnumerable<EventMetrics> domainMetrics
    )
    {
        return domainMetrics.Select(m => m.MapToDbModel()).ToList();
    }
}
