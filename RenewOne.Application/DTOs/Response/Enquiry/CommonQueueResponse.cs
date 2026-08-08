namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class CommonQueueResponse
    {
        public Guid Id { get; set; }
        public string EnquiryId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public string InsuranceType { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByPhone { get; set; } = string.Empty;
        public string CreatedByRole { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public string AgencyNumber { get; set; } = string.Empty;
        public DateTime? SubmittedOn { get; set; }
        public int Priority { get; set; }
    }
}
