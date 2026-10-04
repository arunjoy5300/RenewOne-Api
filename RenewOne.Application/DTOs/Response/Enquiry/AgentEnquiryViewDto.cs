namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public record AgentEnquiryDetailsDto(
    Guid EnquiryId,
    string? EnquiryNumber,

    string? CustomerName,
    string? PhoneNumber,

    string? VehicleNumber,
    string? EngineNumber,
    string? ChassisNumber,

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
    string? AssignedAgentNumber,

    Guid? CreatedById,
    string? CreatedByName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,

    List<DocumentDto> Documents,
    List<AssignmentHistoryDto> AssignmentHistory,

    bool HasQuote,
    bool HasPolicy
);
}
