using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;

namespace RenewOne.Application.Interfaces.LogicService.Accounts
{
    public interface IAccountLogicService
    {
        Task<Result<bool>> RegisterAgentAsync(AgentSignupRequest request);
        Task<Result<bool>> ApproveAgentAsync(Guid accountId);
        Task<Result<IEnumerable<PendingAgentResponse>>> GetPendingAgentsAsync(PendingAgentFilterRequest filter);
        Task<Result<List<AgentLookupResponse>>> GetAgentsAsync(string? search, CancellationToken cancellationToken = default);
    }
}
