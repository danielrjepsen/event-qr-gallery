using Nory.Application.DTOs;
using Nory.Core.Domain.Enums;

namespace Nory.Application.Services;

public interface IMetricsService
{
    Task<AggregatedEventMetricsDto> GetAggregatedMetricsForEventsAsync(List<Guid> eventIds);
    Task<EventMetricsDto> GetEventMetricsAsync(Guid eventId, MetricsPeriodType periodType);
}