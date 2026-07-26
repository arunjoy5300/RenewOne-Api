using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public IQueryable<User> GetAllUsers()
        {
            return _context.Users;
        }
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<User?> GetByLoginIdentifierAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => (u.UserName == identifier || u.Email == identifier));
        }
        public async Task<IEnumerable<User>> GetAgentStaffByAccountAsync(Guid accountId)
        {
            return await _context.Users
                .Where(u => u.AccountId == accountId && u.UserType == "Staff")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
