using Nory.Core.Domain.Entities;
using Nory.Core.Domain.Enums;

namespace Nory.Core.Domain.Repositories;

public interface IAnalyticsRepository
{
    // Activity logs
    Task<ActivityLog> AddActivityAsync(ActivityLog activity);
    Task<List<ActivityLog>> GetActivitiesAsync(Guid eventId);
    Task<List<ActivityLog>> GetActivitiesByTypeAsync(Guid eventId, ActivityType type);
    Task<List<ActivityLog>> GetRecentActivitiesAsync(int count = 10);
    Task<List<ActivityLog>> GetRecentActivitiesForEventsAsync(List<Guid> eventIds, int limit);
    Task<List<ActivityLog>> GetUnprocessedActivitiesAsync(int batchSize = 100);
    Task<int> GetActivityCountByTypeAsync(Guid eventId, ActivityType type);
    Task<DateTime?> GetLastActivityTimeAsync(Guid eventId);
    Task MarkActivitiesAsProcessedAsync(List<Guid> activityIds);

    Task<EventMetrics?> GetMetricsAsync(
        Guid eventId,
        MetricsPeriodType periodType = MetricsPeriodType.Total
    );

    Task<List<EventMetrics>> GetAllMetricsAsync(MetricsPeriodType periodType);

    Task<List<EventMetrics>> GetMetricsByPeriodAsync(MetricsPeriodType periodType);
    Task<List<EventMetrics>> GetMetricsForDateRangeAsync(
        Guid eventId,
        DateTime start,
        DateTime end
    );
    Task<EventMetrics> AddMetricsAsync(EventMetrics metrics);
    Task UpdateMetricsAsync(EventMetrics metrics);
    Task DeleteMetricsAsync(Guid metricsId);
    Task<EventMetrics> UpsertMetricsAsync(EventMetrics metrics);

    // bulk
    Task<List<EventMetrics>> GetMetricsForEventsAsync(
        List<Guid> eventIds,
        MetricsPeriodType periodType
    );
    Task<Dictionary<Guid, int>> GetPhotoCountsForEventsAsync(List<Guid> eventIds);

    // utility
    Task<bool> HasActivityDataAsync(Guid eventId);
    Task<bool> HasMetricsDataAsync(Guid eventId);
    Task SaveChangesAsync();
}
