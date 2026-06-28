using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.UserLedger
{
    public class PolicyPaymentRepository : IPolicyPaymentRepository
    {
        private readonly AppDbContext _dbContext;
        public PolicyPaymentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(PolicyPayment policyPayment)
        {
            await _dbContext.PolicyPayments.AddAsync(policyPayment);
        }
        public IQueryable<PolicyPayment> GetPolicyPaymentQueryable()
        {
            return _dbContext.PolicyPayments;
        }
        public void Update(PolicyPayment policyPayment)
        {
            _dbContext.PolicyPayments.Update(policyPayment);
        }
    }
}
