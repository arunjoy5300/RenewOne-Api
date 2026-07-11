using RenewOne.Application.Hubs.RenewOne.Application.Hubs;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Services.SignalR
{
    public interface INotificationService
    {
        Task SendToRolesAsync(
            NotificationMessage notification,
            params string[] roles);

        Task SendToUsersAsync(
            NotificationMessage notification,
            params Guid[] userIds);

        Task SendToAccountsAsync(
            NotificationMessage notification,
            params Guid[] accountIds);
    }
}
