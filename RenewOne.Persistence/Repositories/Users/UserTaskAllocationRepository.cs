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
        public IQueryable<UserTaskAllocation> GetQueryable()
        {
            return _dbContext.UserTaskAllocations;
        }
    }
}
