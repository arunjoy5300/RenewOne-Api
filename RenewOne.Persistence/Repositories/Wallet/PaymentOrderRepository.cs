using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Wallet
{
    public class PaymentOrderRepository : IPaymentOrderRepository
    {
        private readonly AppDbContext _dbContext;
        public PaymentOrderRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(PaymentOrder paymentOrder)
        {
            await _dbContext.PaymentOrders.AddAsync(paymentOrder);
        }

        public async Task<PaymentOrder?> GetByMerchantOrderIdAsync(string merchantOrderId)
        {
            return await _dbContext.PaymentOrders
                .FirstOrDefaultAsync(po => po.MerchantOrderId == merchantOrderId);
        }

        public IQueryable<PaymentOrder> GetQueryable()
        {
            return _dbContext.PaymentOrders.AsQueryable();
        }

        public void Update(PaymentOrder paymentOrder)
        {
            _dbContext.PaymentOrders.Update(paymentOrder);
        }
    }
}
