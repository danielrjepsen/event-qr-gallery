using Nory.Core.Domain.Entities;
using Nory.Infrastructure.Persistence.Models;

namespace Nory.Infrastructure.Persistence.Extensions;

public static class EventMappingExtensions
{
    // DbModel -> Domain
    public static Event MapToDomain(this EventDbModel dbModel)
    {
        var eventEntity = new Event(dbModel.Name, dbModel.StartsAt, dbModel.EndsAt)
        {
            Id = dbModel.Id,
            Status = dbModel.Status,
            Description = dbModel.Description,
            HasContent = dbModel.HasContent,
            CreatedAt = dbModel.CreatedAt,
            UpdatedAt = dbModel.UpdatedAt,
        };

        return eventEntity;
    }

    // Domain -> DbModel
    public static EventDbModel MapToDbModel(this Event domainEvent)
    {
        return new EventDbModel
        {
            Id = domainEvent.Id,
            Name = domainEvent.Name,
            Description = domainEvent.Description,
            StartsAt = domainEvent.StartsAt,
            EndsAt = domainEvent.EndsAt,
            Status = domainEvent.Status,
            HasContent = domainEvent.HasContent,
            CreatedAt = domainEvent.CreatedAt,
            UpdatedAt = domainEvent.UpdatedAt,
        };
    }

    // Batch
    public static List<Event> MapToDomain(this IEnumerable<EventDbModel> dbModels)
    {
        return dbModels.Select(db => db.MapToDomain()).ToList();
    }
}
