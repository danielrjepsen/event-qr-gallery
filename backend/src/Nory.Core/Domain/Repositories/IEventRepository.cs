namespace Nory.Core.Domain.Repositories;

using Nory.Core.Domain.Entities;

public interface IEventRepository
{
    Task<List<Event>> GetEventsAsync();
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event> AddAsync(Event eventEntity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync();
}