using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Vendor;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.DTOs.Response.Vendor;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Domain.Enums;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Enquiry;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry
{
    public interface IEnquiryLogicService
    {
        Task<Result<PagedResponse<EnquiryListResponse>>> GetEnquiriesAsync(EnquiryFilter enquiryFilter);
        Task<Result<EnquiryDetailsResponse>> GetEnquiryAsync(Guid enquiryId);
        Task<Result<PagedResponse<EnquiryListResponse>>> GetMyEnquiriesAsync(EnquiryFilter enquiryFilter);
        Task<Result<EnquiryDashboardResponse>>  AllEnquiryDashboard(DashboardFilter filter);
        Task<Result<EnquiryDashboardResponse>> MyEnquiryDashboard(DashboardFilter filter);
        Task<Result<PagedResponse<CommonQueueResponse>>> GetCommonQueueAsync(CommonQueueFilter filter, CancellationToken ct);
        Task<Result<string>> AssignAsync(Guid enquiryId,Guid employeeId,string? remarks,CancellationToken cancellationToken = default);
        Task<Result<List<AssignableEmployeeResponse>>> GetAssignableEmployeesAsync(Guid enquiryId,CancellationToken cancellationToken = default);
        Task<Result<EnquiryLedgerDetails>> GetEnquiryLedgerDetails(Guid enquiryId, CancellationToken ct);
        Task<Result<PagedResponse<CommonQueueResponse>>> MyTaskEnquiries(EnquiryFilter filter,CancellationToken ct);
        Task<Result<PagedResponse<QuoteQueueResponse>>> MyQuoteQueue(QuoteQueueFilter filter, CancellationToken ct);
        Task<Result<PagedResponse<QuoteQueueResponse>>> MyPolicyQueue(PolicyQueueFilter filter, CancellationToken ct);
        Task<Result<PagedResponse<QuoteQueueResponse>>> DraftQueue(PolicyQueueFilter filter, CancellationToken ct);
        Task<Result<AdminEnquiryDashboardResponse>> GetAdminEnquiryDashboardAsync(EnquiryDashboardFilter filter, CancellationToken ct);
        Task<Result<PagedResponse<QuoteQueueResponse>>> GetAllAdminEnquiriesAsync(PolicyQueueFilter filter, CancellationToken ct);
        Task<Result<PolicySummaryDto>> GetPolicySummaryAsync(Guid enquiryId, CancellationToken ct);
        Task<Result<PaymentDetailsDto>> GetPaymentDetailsAsync(Guid enquiryId, CancellationToken ct);
        Task<Result<VendorSettlementDetailsDto>> GetVendorSettlementDetailsAsync(Guid enquiryId, Guid vendorId, VendorSettlementMethod settlementMethod, CancellationToken ct);
        Task<Result<PolicyDetailsDto>> CreatePolicyDetailsAsync(Guid enquiryId, CreatePolicyDetailsRequest request, CancellationToken ct);
        Task<Result<PolicyDetailsDto>> GetPolicyDetailsAsync(Guid enquiryId, CancellationToken ct);
        Task<Result<bool>> SubmitVendorSettlementAsync(SubmitVendorSettlementRequest request, CancellationToken ct);
        Task<Result<bool>> IsVendorSettlementCompletedAsync(Guid enquiryId, CancellationToken ct);
        Task<Result<bool>> ChangeEnquiryStatusAsync(ChangeEnquiryStatusRequest request,CancellationToken ct);
        Task<Result<AgentEnquiryDashboardResponse>> GetAgentEnquiriesAsync(AgentEnquiryFilterDto filter, CancellationToken ct = default);
        Task<Result<AgentPolicyPreview>> GetAgentPolicyPreview(Guid EnquiryId, CancellationToken ct);
        Task<Result<AgentEnquiryDetailsDto>> GetAgentEnquiryDetailsAsync(Guid enquiryId, CancellationToken cancellationToken = default);
    }
}
