namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class AdminAssistedEnquiryResponse
    {
        public Guid Id { get; set; }
        public string EnquiryNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public string? VehicleType { get; set; }
        public string? InsuranceType { get; set; }
        public string? PolicyType { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? CurrentStep { get; set; }
        public bool AdminAssisted { get; set; }
        public Guid AccountId { get; set; }
        public Guid? SelectedQuoteId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
