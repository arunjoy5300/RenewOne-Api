using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Users
{
    public class UserTaskAllocationRepository : IUserTaskAllocationRepository
    {
        private readonly AppDbContext _dbContext;
        public UserTaskAllocationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(UserTaskAllocation allocation)
        {
            await _dbContext.UserTaskAllocations.AddAsync(allocation);
        }

        public async Task AddRangeAsync(IEnumerable<UserTaskAllocation> allocations)
        {
            await _dbContext.UserTaskAllocations.AddRangeAsync(allocations);
        }
        public IQueryable<UserTaskAllocation> GetQueryable()
        {
            return _dbContext.UserTaskAllocations;
        }
        public Task DeleteRangeAsync(IEnumerable<UserTaskAllocation> allocations)
        {
            _dbContext.UserTaskAllocations.RemoveRange(allocations);
            return Task.CompletedTask;
        }
    }
}
