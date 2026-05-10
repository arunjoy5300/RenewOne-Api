using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Account;

namespace RenewOne.Application.Interfaces.LogicService.Accounts
{
    public interface IAccountLogicService
    {
        Task<Result<bool>> RegisterAgentAsync(AgentSignupRequest request);
    }
}
