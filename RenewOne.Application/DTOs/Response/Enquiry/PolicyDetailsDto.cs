namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class PolicyDetailsDto
    {
        public Guid Id { get; set; }
        public Guid EnquiryId { get; set; }

        public string PolicyNumber { get; set; } = string.Empty;

        public DateOnly PolicyStartDate { get; set; }

        public DateOnly PolicyEndDate { get; set; }

        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
