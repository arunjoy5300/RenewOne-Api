using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public record AgentEnquiryFilterDto(
    string? Search,
    Guid? InsuranceTypeId,
    Guid? VehicleTypeId,
    string? EnquiryStatus,
    Guid? AgentId,
    DateFilterType DateFilter,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string SortDirection = "Desc",
    int PageNumber = 1,
    int PageSize = 20
);
}
