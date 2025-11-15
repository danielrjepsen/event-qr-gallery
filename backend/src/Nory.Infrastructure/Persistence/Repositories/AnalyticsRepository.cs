using Microsoft.EntityFrameworkCore;
using Nory.Core.Domain.Entities;
using Nory.Core.Domain.Enums;
using Nory.Core.Domain.Repositories;
using Nory.Infrastructure.Persistence.Extensions;

namespace Nory.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // activity logs
    public async Task<ActivityLog> AddActivityAsync(ActivityLog activity)
    {
        var dbModel = activity.MapToDbModel();
        _context.ActivityLogs.Add(dbModel);
        return activity;
    }

    public async Task<List<ActivityLog>> GetActivitiesAsync(Guid eventId)
    {
        var dbModels = await _context
            .ActivityLogs.Where(a => a.EventId == eventId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<List<ActivityLog>> GetActivitiesByTypeAsync(Guid eventId, ActivityType type)
    {
        var dbModels = await _context
            .ActivityLogs.Where(a => a.EventId == eventId && a.Type == type)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<int> GetActivityCountByTypeAsync(Guid eventId, ActivityType type)
    {
        return await _context
            .ActivityLogs.Where(a => a.EventId == eventId && a.Type == type)
            .CountAsync();
    }

    public async Task<List<ActivityLog>> GetRecentActivitiesAsync(int limit)
    {
        var dbModels = await _context
            .ActivityLogs.Include(a => a.Event)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<List<ActivityLog>> GetRecentActivitiesForEventsAsync(
        List<Guid> eventIds,
        int limit
    )
    {
        var dbModels = await _context
            .ActivityLogs.Where(a => eventIds.Contains(a.EventId))
            .Include(a => a.Event)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<List<ActivityLog>> GetUnprocessedActivitiesAsync(int batchSize = 100)
    {
        var dbModels = await _context
            .ActivityLogs.Where(a => !a.IsProcessed)
            .OrderBy(a => a.CreatedAt)
            .Take(batchSize)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task MarkActivitiesAsProcessedAsync(List<Guid> activityIds)
    {
        await _context
            .ActivityLogs.Where(a => activityIds.Contains(a.Id))
            .ExecuteUpdateAsync(a => a.SetProperty(x => x.IsProcessed, true));
    }

    public async Task<DateTime?> GetLastActivityTimeAsync(Guid eventId)
    {
        return await _context
            .ActivityLogs.Where(a => a.EventId == eventId)
            .MaxAsync(a => (DateTime?)a.CreatedAt);
    }

    // event metrics
    public async Task<EventMetrics?> GetMetricsAsync(
        Guid eventId,
        MetricsPeriodType periodType = MetricsPeriodType.Total
    )
    {
        var dbModel = await _context
            .EventMetrics.Where(m => m.EventId == eventId && m.PeriodType == periodType)
            .FirstOrDefaultAsync();

        return dbModel?.MapToDomain();
    }

    public async Task<List<EventMetrics>> GetMetricsByPeriodAsync(MetricsPeriodType periodType)
    {
        var dbModels = await _context
            .EventMetrics.Where(m => m.PeriodType == periodType)
            .OrderByDescending(m => m.UpdatedAt)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<List<EventMetrics>> GetAllMetricsAsync(MetricsPeriodType periodType)
    {
        var dbModels = await _context
            .EventMetrics.Where(m => m.PeriodType == periodType)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<List<EventMetrics>> GetMetricsForDateRangeAsync(
        Guid eventId,
        DateTime start,
        DateTime end
    )
    {
        var dbModels = await _context
            .EventMetrics.Where(m =>
                m.EventId == eventId && m.PeriodStart >= start && m.PeriodEnd <= end
            )
            .OrderBy(m => m.PeriodStart)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<EventMetrics> AddMetricsAsync(EventMetrics metrics)
    {
        var dbModel = metrics.MapToDbModel();
        _context.EventMetrics.Add(dbModel);
        return metrics;
    }

    public async Task UpdateMetricsAsync(EventMetrics metrics)
    {
        var existingDbModel = await _context.EventMetrics.FindAsync(metrics.Id);

        if (existingDbModel != null)
        {
            var updatedDbModel = metrics.MapToDbModel();

            existingDbModel.TotalPhotosUploaded = updatedDbModel.TotalPhotosUploaded;
            existingDbModel.TotalGuestAppOpens = updatedDbModel.TotalGuestAppOpens;
            existingDbModel.TotalQrScans = updatedDbModel.TotalQrScans;
            existingDbModel.TotalSlideshowViews = updatedDbModel.TotalSlideshowViews;
            existingDbModel.TotalGalleryViews = updatedDbModel.TotalGalleryViews;
            existingDbModel.LiveGuestCount = updatedDbModel.LiveGuestCount;
            existingDbModel.FeatureUsage = updatedDbModel.FeatureUsage;
            existingDbModel.UpdatedAt = updatedDbModel.UpdatedAt;

            _context.EventMetrics.Update(existingDbModel);
        }
    }

    public async Task<EventMetrics> UpsertMetricsAsync(EventMetrics metrics)
    {
        var existing = await _context.EventMetrics.FirstOrDefaultAsync(m =>
            m.EventId == metrics.EventId && m.PeriodType == metrics.PeriodType
        );

        if (existing != null)
        {
            var updatedDbModel = metrics.MapToDbModel();
            existing.TotalPhotosUploaded = updatedDbModel.TotalPhotosUploaded;
            existing.TotalGuestAppOpens = updatedDbModel.TotalGuestAppOpens;
            existing.TotalQrScans = updatedDbModel.TotalQrScans;
            existing.TotalSlideshowViews = updatedDbModel.TotalSlideshowViews;
            existing.TotalGalleryViews = updatedDbModel.TotalGalleryViews;
            existing.LiveGuestCount = updatedDbModel.LiveGuestCount;
            existing.FeatureUsage = updatedDbModel.FeatureUsage;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.EventMetrics.Update(existing);
        }
        else
        {
            var dbModel = metrics.MapToDbModel();
            _context.EventMetrics.Add(dbModel);
        }

        return metrics;
    }

    public async Task DeleteMetricsAsync(Guid metricsId)
    {
        var dbModel = await _context.EventMetrics.FindAsync(metricsId);
        if (dbModel != null)
        {
            _context.EventMetrics.Remove(dbModel);
        }
    }

    // bulk
    public async Task<List<EventMetrics>> GetMetricsForEventsAsync(
        List<Guid> eventIds,
        MetricsPeriodType periodType
    )
    {
        var dbModels = await _context
            .EventMetrics.Where(m => eventIds.Contains(m.EventId) && m.PeriodType == periodType)
            .ToListAsync();

        return dbModels.MapToDomain();
    }

    public async Task<Dictionary<Guid, int>> GetPhotoCountsForEventsAsync(List<Guid> eventIds)
    {
        return await _context
            .EventPhotos.Where(p => eventIds.Contains(p.EventId))
            .GroupBy(p => p.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count);
    }

    // utility
    public async Task<bool> HasActivityDataAsync(Guid eventId)
    {
        return await _context.ActivityLogs.AnyAsync(a => a.EventId == eventId);
    }

    public async Task<bool> HasMetricsDataAsync(Guid eventId)
    {
        return await _context.EventMetrics.AnyAsync(m => m.EventId == eventId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
