namespace RenewOne.Application.DTOs.Request.Enquiry
{
    public class CreatePolicyDetailsRequest
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public DateOnly PolicyStartDate { get; set; }
        public DateOnly PolicyEndDate { get; set; }
        public string? Remarks { get; set; }
    }
}
