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
            return await _dbContext.AgentClasses.Include(x => x.Accounts).ThenInclude(x => x.Users).ToListAsync();
        }

        public async Task<AgentClass?> GetByIdAsync(Guid id)
        {
            return await _dbContext.AgentClasses.Include(x => x.Accounts).ThenInclude(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);
        }
        public Task<bool> ExistsByNameAsync(string className, Guid? excludingId = null) =>
            _dbContext.AgentClasses.AnyAsync(x => x.ClassName.ToLower() == className.ToLower() && (!excludingId.HasValue || x.Id != excludingId.Value));

        public async Task<bool> AddAsync(AgentClass agentClass)
        {
            await _dbContext.AgentClasses.AddAsync(agentClass);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(AgentClass agentClass)
        {
            _dbContext.AgentClasses.Update(agentClass);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleStatusAsync(AgentClass agentClass)
        {
            agentClass.IsActive = !agentClass.IsActive;
            agentClass.UpdatedAt = DateTime.UtcNow;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        public IQueryable<AgentClass> GetQueryable()
        {
            return _dbContext.AgentClasses.AsQueryable();
        }

    }
}
