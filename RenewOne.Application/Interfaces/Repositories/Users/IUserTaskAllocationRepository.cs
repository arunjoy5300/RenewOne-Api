using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Users
{
    public interface IUserTaskAllocationRepository
    {
        IQueryable<UserTaskAllocation> GetQueryable();
    }
}
