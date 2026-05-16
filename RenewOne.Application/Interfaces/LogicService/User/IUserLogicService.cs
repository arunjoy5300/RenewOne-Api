using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.User;
using RenewOne.Application.DTOs.Response.User;

namespace RenewOne.Application.Interfaces.LogicService.User
{
    public interface IUserLogicService
    {
        Task<Result<bool>> CreateStaffAsync(CreateStaffRequest request, Guid accountId);
        Task<Result<IEnumerable<StaffResponse>>> GetMyStaffAsync(Guid accountId);
        Task<Result<bool>> ToggleStaffStatusAsync(Guid staffId, Guid accountId);
    }
}
