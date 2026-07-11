using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.BankAccounts
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly AppDbContext _dbContext;
        public BankAccountRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(BankAccount bankAccount)
        {
            await _dbContext.BankAccount.AddAsync(bankAccount);
        }

        public IQueryable<BankAccount> GetQueryable()
        {
            return _dbContext.BankAccount.AsQueryable();
        }

        public async Task<BankAccount?> GetByIdAsync(Guid id)
        {
            return await _dbContext.BankAccount
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public void Update(BankAccount bankAccount)
        {
            _dbContext.BankAccount.Update(bankAccount);
        }
    }
}
