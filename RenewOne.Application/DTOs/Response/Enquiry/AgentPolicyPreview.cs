namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public record AgentPolicyPreview(
        string EnquiryNumber,
        string InsuredCompanyName,
        string PolicyNumber,
        DateOnly PolicyStartDate,
        DateOnly PolicyEndDate,
        string PolicyUrl,
        string? PolicyIssuedBy,
        string? PolicyIssuedByPhone
    );
}
