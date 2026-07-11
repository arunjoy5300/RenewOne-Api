using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RenewOne.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var accountId = Context.User?.FindFirst("AccountId")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"User_{userId}");
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    role);
            }

            if (!string.IsNullOrWhiteSpace(accountId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"Account_{accountId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var accountId = Context.User?.FindFirst("AccountId")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"User_{userId}");
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    role);
            }

            if (!string.IsNullOrWhiteSpace(accountId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"Account_{accountId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
