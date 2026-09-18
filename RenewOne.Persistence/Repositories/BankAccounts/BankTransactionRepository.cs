using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.BankAccounts
{
    public class BankTransactionRepository : IBankTransactionRepository
    {
        private readonly AppDbContext _dbContext;
        public BankTransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<BankTransaction> GetQueryable()
        {
            return _dbContext.BankTransactions
                .AsQueryable();
        }

        public async Task<BankTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken ct)
        {
            return await _dbContext.BankTransactions
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task AddAsync(
            BankTransaction transaction,
            CancellationToken ct)
        {
            await _dbContext.BankTransactions.AddAsync(
                transaction,
                ct);
        }

        public void Update(BankTransaction transaction)
        {
            _dbContext.BankTransactions.Update(transaction);
        }

        public void Delete(BankTransaction transaction)
        {
            _dbContext.BankTransactions.Remove(transaction);
        }
    }
}
