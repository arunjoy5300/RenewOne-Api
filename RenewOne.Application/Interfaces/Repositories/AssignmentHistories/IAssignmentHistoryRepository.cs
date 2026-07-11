using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AssignmentHistories
{
    public interface IAssignmentHistoryRepository
    {
        Task AddAsync(AssignmentHistory history);
        IQueryable<AssignmentHistory> GetQueryable();
    }
}
