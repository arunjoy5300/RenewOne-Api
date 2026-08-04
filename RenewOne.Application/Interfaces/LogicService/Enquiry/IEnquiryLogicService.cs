using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.Filters.Enquiry;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry
{
    public interface IEnquiryLogicService
    {
        Task<Result<PagedResponse<EnquiryListResponse>>> GetEnquiriesAsync(EnquiryFilter enquiryFilter);
        Task<Result<EnquiryDetailsResponse>> GetEnquiryAsync(Guid enquiryId);
        Task<Result<PagedResponse<EnquiryListResponse>>> GetMyEnquiriesAsync(EnquiryFilter enquiryFilter);
        Task<Result<EnquiryDashboardResponse>>  AllEnquiryDashboard(DashboardFilter filter);
        Task<Result<EnquiryDashboardResponse>> MyEnquiryDashboard(DashboardFilter filter);
        Task<Result<PagedResponse<CommonQueueResponse>>> GetCommonQueueAsync(CommonQueueFilter filter);
        Task<Result<PagedResponse<CommonQueueResponse>>> GetMyCommonQueueAsync(CommonQueueFilter filter);
        Task<Result<string>> AssignAsync(Guid enquiryId,Guid employeeId,string? remarks,CancellationToken cancellationToken = default);
        Task<Result<List<AssignableEmployeeResponse>>> GetAssignableEmployeesAsync(Guid enquiryId,CancellationToken cancellationToken = default);
        Task<Result<EnquiryLedgerDetails>> GetEnquiryLedgerDetails(Guid enquiryId, CancellationToken ct);
    }
}
