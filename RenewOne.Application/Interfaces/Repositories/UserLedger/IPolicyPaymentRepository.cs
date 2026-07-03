using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.UserLedger
{
    public interface IPolicyPaymentRepository
    {
        Task AddAsync(PolicyPayment policyPayment);
        IQueryable<PolicyPayment> GetPolicyPaymentQueryable();
        void Update(PolicyPayment policyPayment);
        IQueryable<PolicyPayment> GetQueryable();
        Task<PolicyPayment?> GetByIdAsync(Guid id);
    }
}
