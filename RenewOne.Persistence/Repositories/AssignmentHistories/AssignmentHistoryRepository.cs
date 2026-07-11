using RenewOne.Application.Interfaces.Repositories.AssignmentHistories;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AssignmentHistories
{
    public class AssignmentHistoryRepository : IAssignmentHistoryRepository
    {
        private readonly AppDbContext _context;

        public AssignmentHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AssignmentHistory history)
        {
            await _context.AssignmentHistories.AddAsync(history);
        }

        public IQueryable<AssignmentHistory> GetQueryable()
        {
            return _context.AssignmentHistories.AsQueryable();
        }
    }
}
