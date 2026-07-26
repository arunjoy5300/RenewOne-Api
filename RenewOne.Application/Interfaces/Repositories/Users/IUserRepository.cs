using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Users
{
    public interface IUserRepository
    {
        IQueryable<User> GetAllUsers();
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByLoginIdentifierAsync(string identifier);
        Task AddUserAsync(User user);
        Task<IEnumerable<User>> GetAgentStaffByAccountAsync(Guid accountId);
        Task<User?> GetByIdAsync(Guid id);
        Task UpdateUserAsync(User user);
    }
}
