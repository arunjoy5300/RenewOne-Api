namespace RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry
{
    public record AgentEnquiryListDto(
    Guid EnquiryId,
    string? EnquiryNumber,
    string? CustomerName,
    string? PhoneNumber,
    string? VehicleNumber,
    Guid? InsuranceTypeId,
    string? InsuranceType,
    string? PolicyType,
    Guid? VehicleTypeId,
    string? VehicleType,
    string? Status,
    Guid? AgentId,
    string? AgentName,
    Guid? AssignedAgentId,
    string? AssignedAgentName,
    DateTime CreatedAt
);
}
