using Microsoft.AspNetCore.SignalR;
using RenewOne.API.Hubs;
using RenewOne.Application.Hubs.RenewOne.Application.Hubs;
using RenewOne.Application.Interfaces.Services.SignalR;
using RenewOne.Domain.Entities;

namespace RenewOne.Infrastructure.Services.SignalR
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToRolesAsync(
            NotificationMessage notification,
            params string[] roles)
        {
            if (roles == null || roles.Length == 0)
                return;

            await _hubContext.Clients
                .Groups(roles)
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendToUsersAsync(
            NotificationMessage notification,
            params Guid[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return;

            var groups = userIds
                .Select(x => $"User_{x}")
                .ToArray();

            await _hubContext.Clients
                .Groups(groups)
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendToAccountsAsync(
            NotificationMessage notification,
            params Guid[] accountIds)
        {
            if (accountIds == null || accountIds.Length == 0)
                return;

            var groups = accountIds
                .Select(x => $"Account_{x}")
                .ToArray();

            await _hubContext.Clients
                .Groups(groups)
                .SendAsync("ReceiveNotification", notification);
        }
    }
}