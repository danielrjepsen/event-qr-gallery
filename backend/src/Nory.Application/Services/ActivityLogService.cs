using Nory.Application.DTOs;
using Nory.Application.Extensions;
using Nory.Core.Domain.Entities;
using Nory.Core.Domain.Enums;
using Nory.Core.Domain.Repositories;
using System.Text.Json;

namespace Nory.Application.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(
        IAnalyticsRepository analyticsRepository,
        ILogger<ActivityLogService> logger)
    {
        _analyticsRepository = analyticsRepository;
        _logger = logger;
    }

    public async Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int limit = 20)
    {
        var activities = await _analyticsRepository.GetRecentActivitiesAsync(limit);
        return activities.MapToDtos();
    }

    public async Task<List<ActivityLogDto>> GetRecentActivitiesForEventsAsync(
        List<Guid> eventIds,
        int limit = 20)
    {
        if (!eventIds.Any())
        {
            return new List<ActivityLogDto>();
        }

        var activities = await _analyticsRepository.GetRecentActivitiesForEventsAsync(
            eventIds,
            limit
        );

        return activities.MapToDtos();
    }

    public async Task RecordActivityAsync(
        Guid eventId,
        ActivityType type,
        JsonDocument? data = null,
        string? sessionId = null)
    {
        var activity = new ActivityLog(eventId, type, data, sessionId);
        await _analyticsRepository.AddActivityAsync(activity);
        await _analyticsRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Recorded {ActivityType} for event {EventId}", 
            type, 
            eventId
        );
    }
}