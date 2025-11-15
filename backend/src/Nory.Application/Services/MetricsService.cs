using Nory.Application.DTOs;
using Nory.Application.Extensions;
using Nory.Core.Domain.Enums;
using Nory.Core.Domain.Repositories;

namespace Nory.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        IAnalyticsRepository analyticsRepository,
        ILogger<MetricsService> logger)
    {
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task<AggregatedEventMetricsDto> GetAggregatedMetricsForEventsAsync(
        List<Guid> eventIds)
    {
        if (!eventIds.Any())
        {
            return new AggregatedEventMetricsDto();
        }

        var allMetrics = await _analyticsRepository.GetMetricsForEventsAsync(
            eventIds,
            MetricsPeriodType.Total
        );

        if (!allMetrics.Any())
        {
            _logger.LogWarning(
                "No metrics found for events: {EventIds}",
                string.Join(", ", eventIds)
            );

            return new AggregatedEventMetricsDto();
        }

        return new AggregatedEventMetricsDto
        {
            TotalPhotosUploaded = allMetrics.Sum(m => m.TotalPhotosUploaded),
            TotalGuestAppOpens = allMetrics.Sum(m => m.TotalGuestAppOpens),
            TotalQrScans = allMetrics.Sum(m => m.TotalQrScans),
            TotalSlideshowViews = allMetrics.Sum(m => m.TotalSlideshowViews),
            TotalGalleryViews = allMetrics.Sum(m => m.TotalGalleryViews),
            LiveGuestCount = allMetrics.Sum(m => m.LiveGuestCount),
            ActiveEvents = allMetrics.Count(m => m.LiveGuestCount > 0),
        };
    }

    public async Task<EventMetricsDto> GetEventMetricsAsync(
        Guid eventId,
        MetricsPeriodType periodType)
    {
        var metrics = await _analyticsRepository.GetMetricsAsync(eventId, periodType);

        if (metrics == null)
        {
            _logger.LogWarning(
                "No metrics found for event {EventId} with period {PeriodType}",
                eventId,
                periodType
            );

            return new EventMetricsDto { EventId = eventId };
        }

        return metrics.MapToDto();
    }
}