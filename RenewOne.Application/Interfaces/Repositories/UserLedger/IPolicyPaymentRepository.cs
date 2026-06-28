using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.UserLedger
{
    public interface IPolicyPaymentRepository
    {
        Task AddAsync(PolicyPayment policyPayment);
        IQueryable<PolicyPayment> GetPolicyPaymentQueryable();
        void Update(PolicyPayment policyPayment);
    }
}
