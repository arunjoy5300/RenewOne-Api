using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Wallet
{
    public interface IPaymentOrderRepository
    {
        Task AddAsync(PaymentOrder paymentOrder);
        Task<PaymentOrder?> GetByMerchantOrderIdAsync(string merchantOrderId);
        void Update(PaymentOrder paymentOrder);
        IQueryable<PaymentOrder> GetQueryable();
    }
}
