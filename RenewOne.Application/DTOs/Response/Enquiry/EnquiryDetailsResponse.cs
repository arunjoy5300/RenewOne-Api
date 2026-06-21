namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class EnquiryDetailsResponse
    {
        public Guid Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerMobile { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;

        public string VehicleNumber { get; set; } = string.Empty;

        public string ManufacturerName { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string FuelType { get; set; } = string.Empty;

        public string CubicCapacity { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string AssignedAgent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DocumentsCount { get; set; }

        public int QuotesCount { get; set; }

        public List<DocumentDto> Documents { get; set; } = [];

        public List<QuoteDto> Quotes { get; set; } = [];
    }
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string DocType { get; set; } = string.Empty;
        public string DocSide { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
    public class QuoteDto
    {
        public Guid Id { get; set; }
        //public Guid InsuranceCompanyId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public decimal? IDV { get; set; }
        public decimal Premium { get; set; }
        public decimal AgentPoints { get; set; }
        public string? Remarks { get; set; }
        public DateTime? QuoteDate { get; set; }
    }
}
