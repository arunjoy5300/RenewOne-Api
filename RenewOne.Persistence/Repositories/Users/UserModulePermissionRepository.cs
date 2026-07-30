using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Users
{
    public class UserModulePermissionRepository : IUserModulePermissionRepository

    {
        private readonly AppDbContext _context;
        public UserModulePermissionRepository(AppDbContext dbContext)
        {
            _context = dbContext;
        }
        public IQueryable<UserModulePermission> GetAll()
        {
            return _context.UserModulePermissions.AsQueryable();
        }

        public async Task AddAsync(UserModulePermission permission)
        {
            await _context.UserModulePermissions.AddAsync(permission);
        }

        public async Task AddRangeAsync(IEnumerable<UserModulePermission> permissions)
        {
            await _context.UserModulePermissions.AddRangeAsync(permissions);
        }

        public Task DeleteRangeAsync(IEnumerable<UserModulePermission> permissions)
        {
            _context.UserModulePermissions.RemoveRange(permissions);
            return Task.CompletedTask;
        }
        public async Task<List<UserModulePermission>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserModulePermissions
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}
