using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Employees;
using RenewOne.Application.DTOs.Request.User;
using RenewOne.Application.DTOs.Response.Employees;
using RenewOne.Application.DTOs.Response.User;
using RenewOne.Application.Filters.User;

namespace RenewOne.Application.Interfaces.LogicService.User
{
    public interface IUserLogicService
    {
        Task<Result<bool>> CreateStaffAsync(CreateStaffRequest request, Guid accountId);
        Task<Result<List<StaffResponse>>> GetMyStaffAsync(Guid accountId);
        Task<Result<PagedResponse<StaffResponse>>> ListStaffWithFiltersAsync(Guid accountId,StaffFilterRequest request);
        Task<Result<bool>> ToggleStaffStatusAsync(Guid staffId, Guid accountId);
        Task<Result<bool>> CreateInternalUserAsync(CreateInternalUserRequest request);
        Task<Result<PagedResponse<EmployeeListResponse>>> GetAdminEmployee(EmployeeListRequest request);
        Task<Result<bool>> ToggleAdminEmployeeStatusAsync(Guid employeeId);
        Task<Result<AdminStaffDashboardResponse>> GetAdminStaffDashboardOverviewAsync();
        Task<Result<EmployeeRightsResponse>> GetAdminEmployeeRightsAsync(Guid employeeId);
        Task<Result<bool>> UpdateAdminEmployeeRightsAsync(UpdateEmployeeRightsRequest request);
        Task<Result<bool>> UpdateAdminEmployeeAsync(UpdateAdminEmployeeRequest request);
    }
}
