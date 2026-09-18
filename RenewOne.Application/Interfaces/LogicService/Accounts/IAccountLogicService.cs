using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;

namespace RenewOne.Application.Interfaces.LogicService.Accounts
{
    public interface IAccountLogicService
    {
        Task<Result<bool>> RegisterAgentAsync(AgentSignupRequest request);
        Task<Result<bool>> ApproveAgentAsync(Guid accountId);
        Task<Result<AgentDashboardResponse>> GetAgentsAsync(AgentFilterRequest? request,CancellationToken ct);
        Task<Result<PagedResponse<AgentLookupResponse>>> GetAgentLookupAsync(AgentLookupRequest request);
        Task<Result<UserProfileResponse>> GetUserProfileAsync(Guid userId);
        Task<Result<AgentAdminDetailsResponse>> GetAgentDetailsByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<Result<AgentDashboardOverviewResponse>> GetAgentDashboardOverviewAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<Result<List<AgentDropdownDto>>> GetAgentDropdownAsync(CancellationToken cancellationToken = default);
    }
}
