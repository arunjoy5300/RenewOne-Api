using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Class
{
    public interface IAgentClassRepository
    {
        Task<IEnumerable<AgentClass>> GetAllAsync();
        Task<AgentClass?> GetByIdAsync(Guid id);
        Task<bool> ExistsByNameAsync(string className, Guid? excludingId = null);
        Task<bool> AddAsync(AgentClass agentClass);
        Task<bool> UpdateAsync(AgentClass agentClass);
        Task<bool> ToggleStatusAsync(AgentClass agentClass);
        IQueryable<AgentClass> GetQueryable();
    }
}
