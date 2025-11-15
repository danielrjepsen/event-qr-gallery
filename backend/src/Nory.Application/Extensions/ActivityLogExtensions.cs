using Nory.Application.DTOs;
using Nory.Core.Domain.Entities;

namespace Nory.Application.Extensions;

public static class ActivityLogExtensions
{
    public static ActivityLogDto MapToDto(this ActivityLog activity) =>
        new()
        {
            Id = activity.Id.ToString(),
            Type = activity.Type.ToString(),
            EventId = activity.EventId,
            EventName = activity.GetEventName(),
            Description = activity.GetDescription(),
            Timestamp = activity.CreatedAt,
            Data = activity.Data,
        };

    public static List<ActivityLogDto> MapToDtos(this IEnumerable<ActivityLog> activities) =>
        activities.Select(a => a.MapToDto()).ToList();
}
