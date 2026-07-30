using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Users
{
    public interface IUserTaskAllocationRepository
    {
        Task AddAsync(UserTaskAllocation allocation);
        Task AddRangeAsync(IEnumerable<UserTaskAllocation> allocations);
        IQueryable<UserTaskAllocation> GetQueryable();
        Task DeleteRangeAsync(IEnumerable<UserTaskAllocation> allocations);
    }
}
