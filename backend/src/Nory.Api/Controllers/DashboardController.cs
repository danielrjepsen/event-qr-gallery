using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Nory.Application.Services;

namespace Nory.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IMetricsService _metricsService;
    private readonly IActivityLogService _activityLogService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DashboardController> _logger;

    private const int DashboardOverviewCacheTtlSeconds = 60;

    public DashboardController(
        IEventService eventService,
        IMetricsService metricsService,
        IActivityLogService activityLogService,
        IDistributedCache cache,
        ILogger<DashboardController> logger
    )
    {
        _eventService = eventService;
        _metricsService = metricsService;
        _activityLogService = activityLogService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get all dashboard data
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetDashboardOverview()
    {
        try
        {
            var cacheKey = "dashboard:overview";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Returning dashboard data from cache");
                return Ok(JsonSerializer.Deserialize<DashboardOverviewDto>(cachedData));
            }

            var events = await _eventService.GetEventsAsync();

            if (!events.Any())
            {
                return Ok(new DashboardOverviewDto());
            }

            var eventIds = events.Select(e => e.Id).ToList();

            var metricsTask = _metricsService.GetAggregatedMetricsForEventsAsync(eventIds);
            var activitiesTask = _activityLogService.GetRecentActivitiesForEventsAsync(
                eventIds,
                20
            );

            await Task.WhenAll(metricsTask, activitiesTask);

            var result = new DashboardOverviewDto
            {
                Events = events,
                Analytics = metricsTask.Result,
                RecentActivities = activitiesTask.Result,
            };

            await CacheResultAsync(cacheKey, result, DashboardOverviewCacheTtlSeconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard overview");
            return StatusCode(500, new { error = "Failed to load dashboard data" });
        }
    }

    /// <summary>
    /// cache result
    /// </summary>
    private async Task CacheResultAsync<T>(string key, T data, int ttlSeconds)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds),
            };

            var jsonData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(key, jsonData, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache data for key {Key}", key);
        }
    }
}
