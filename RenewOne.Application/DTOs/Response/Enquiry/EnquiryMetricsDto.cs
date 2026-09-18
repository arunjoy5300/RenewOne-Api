using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;

namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public record EnquiryMetricsDto(
        int AllEnquiries,
        int Created,
        int Submitted,
        int Quoted,
        int PolicyIssued,
        decimal PointsEarned
    );

    public class AgentEnquiryDashboardResponse
    {
        public EnquiryMetricsDto Metrics { get; set; } = default!;
        public PagedResponse<AgentEnquiryListDto> Enquiries { get; set; } = default!;
    }
}
