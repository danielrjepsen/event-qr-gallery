using System.Text.Json;
using Nory.Core.Domain.Enums;

namespace Nory.Application.Services;

public interface IActivityLogService
{
    Task<List<ActivityLogDto>> GetRecentActivitiesAsync(int limit = 20);
    Task<List<ActivityLogDto>> GetRecentActivitiesForEventsAsync(
        List<Guid> eventIds,
        int limit = 20
    );
    Task RecordActivityAsync(
        Guid eventId,
        ActivityType type,
        JsonDocument? data = null,
        string? sessionId = null
    );
}
