using Nory.Application.DTOs;

public class DashboardOverviewDto
{
    public List<EventDto> Events { get; set; } = new();
    public AggregatedEventMetricsDto Analytics { get; set; } = new();
    public List<ActivityLogDto> RecentActivities { get; set; } = new();
}