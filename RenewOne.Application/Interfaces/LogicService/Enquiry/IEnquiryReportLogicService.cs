using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Reports;

namespace RenewOne.Application.Interfaces.LogicService.Enquiry
{
    public interface IEnquiryReportLogicService
    {
        Task<EnquiryReportResult> GenerateAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken);
    }
}
