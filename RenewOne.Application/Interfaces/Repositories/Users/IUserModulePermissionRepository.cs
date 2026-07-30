using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Users
{
    public interface IUserModulePermissionRepository
    {
        IQueryable<UserModulePermission> GetAll();
        Task AddAsync(UserModulePermission permission);
        Task AddRangeAsync(IEnumerable<UserModulePermission> permissions);
        Task DeleteRangeAsync(IEnumerable<UserModulePermission> permissions);
        Task<List<UserModulePermission>> GetByUserIdAsync(Guid userId);
    }
}
