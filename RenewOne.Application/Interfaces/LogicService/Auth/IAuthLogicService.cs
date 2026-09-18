using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Auth;
using RenewOne.Application.DTOs.Response.Auth;

namespace RenewOne.Application.Interfaces.LogicService.Auth
{
    public interface IAuthLogicService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
        Task<Result<GeneratedPasswordResetResponse>> ResetAgentPasswordWithGeneratedAsync(Guid accountId, CancellationToken cancellationToken = default);
    }
}
