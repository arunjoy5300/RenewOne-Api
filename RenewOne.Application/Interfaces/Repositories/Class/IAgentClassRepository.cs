using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Class
{
    public interface IAgentClassRepository
    {
        Task<IEnumerable<AgentClass>> GetAllAsync();
        Task<AgentClass?> GetByIdAsync(Guid id);
        IQueryable<AgentClass> GetQueryable();
    }
}
