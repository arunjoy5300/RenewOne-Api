using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAccountAsync(Account account)
        {
            await _context.SaveChangesAsync();
            await _context.Accounts.AddAsync(account);
        }
    }
}
