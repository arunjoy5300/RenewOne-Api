using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> ApproveAgentAsync(Guid accountId)
        {
            var account = await _context.Accounts.Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return false;

            account.IsActive = true;
            foreach (var user in account.Users)
            {
                user.IsActive = true;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Account?> GetByIdAsync(Guid id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<IEnumerable<Account>> GetPendingAgentsWithFilterAsync(string filterType, DateTime? customDate)
        {
            var query = _context.Accounts
                .Include(a => a.AgentClass)
                .Where(a => a.IsActive == false)
                .AsQueryable();

            var today = DateTime.Today;

            query = filterType.ToLower() switch
            {
                "today" => query.Where(a => a.CreatedAt >= today),
                "yesterday" => query.Where(a => a.CreatedAt >= today.AddDays(-1) && a.CreatedAt < today),
                "custom" when customDate.HasValue => query.Where(a => a.CreatedAt.Date == customDate.Value.Date),
                _ => query
            };

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public IQueryable<Account> GetQueryable()
        {
            return _context.Accounts.AsQueryable();
        }
    }
}
