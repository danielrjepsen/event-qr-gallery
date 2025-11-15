// Infrastructure/Jobs/MetricsUpdateJob.cs
using Nory.Application.Services;

namespace Nory.Infrastructure.Jobs;

public class MetricsUpdateJob
{
    private readonly IMetricsService _metricsService;
    private readonly IEventService _eventService;
    private readonly ILogger<MetricsUpdateJob> _logger;

    public MetricsUpdateJob(
        IMetricsService metricsService,
        IEventService eventService,
        ILogger<MetricsUpdateJob> logger
    )
    {
        _metricsService = metricsService;
        _eventService = eventService;
        _logger = logger;
    }

    public async Task UpdateAllMetricsAsync()
    {
        _logger.LogInformation("Starting metrics update job");

        try
        {
            var events = await _eventService.GetEventsAsync();
            var eventIds = events.Select(e => e.Id).ToList();

            if (!eventIds.Any())
            {
                _logger.LogInformation("No events to update metrics for");
                return;
            }

            await _metricsService.UpdateMetricsForEventsAsync(eventIds);
            _logger.LogInformation("Metrics updated for {EventCount} events", eventIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update metrics");
            throw; // Hangfire will retry
        }
    }
}
