using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Class;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Class
{
    public class AgentClassRepository : IAgentClassRepository
    {
        private readonly AppDbContext _dbContext;
        public AgentClassRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<AgentClass>> GetAllAsync()
        {
            return await _dbContext.AgentClasses.ToListAsync();
        }

        public async Task<AgentClass?> GetByIdAsync(Guid id)
        {
            return await _dbContext.AgentClasses.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
