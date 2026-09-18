using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Enquiry.PolicyDetails;

namespace RenewOne.Persistence.Repositories.Enquiry.PolicyDetails
{
    public class PolicyDetailsRepository : IPolicyDetailsRepository
    {
        private readonly AppDbContext _dbContext;
        public PolicyDetailsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<Domain.Entities.PolicyDetails> GetQueryable()
        {
            return _dbContext.PolicyDetails
                .AsNoTracking();
        }

        public async Task<Domain.Entities.PolicyDetails?> GetByIdAsync(
            Guid id,
            CancellationToken ct)
        {
            return await _dbContext.PolicyDetails
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task AddAsync(
            Domain.Entities.PolicyDetails entity,
            CancellationToken ct)
        {
            await _dbContext.PolicyDetails.AddAsync(entity, ct);
        }

        public void Update(Domain.Entities.PolicyDetails entity)
        {
            _dbContext.PolicyDetails.Update(entity);
        }
    }
}
