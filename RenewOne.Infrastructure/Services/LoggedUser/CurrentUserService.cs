using Microsoft.AspNetCore.Http;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using System.Security.Claims;

namespace RenewOne.Infrastructure.Services.User
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId =>
            Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public Guid AccountId =>
            Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("AccountId")!.Value);

        public string Role =>
            _httpContextAccessor.HttpContext!.User.FindFirst("Role")!.Value;

        public string? IpAddress =>
            _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString();
    }
}
