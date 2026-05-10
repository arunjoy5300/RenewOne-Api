using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<User?> GetByLoginIdentifierAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => (u.UserName == identifier || u.Email == identifier) && u.IsActive);
        }
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
