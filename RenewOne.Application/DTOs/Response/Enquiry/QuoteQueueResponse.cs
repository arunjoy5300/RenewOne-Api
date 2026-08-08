using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class QuoteQueueResponse
    {
        public Guid Id { get; set; }

        public string EnquiryId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerNumber { get; set; } = string.Empty;

        public string? VehicleNumber { get; set; }

        public string? VehicleType { get; set; }

        public string? InsuranceType { get; set; }

        public string? PolicyType { get; set; }

        public string? CreatedBy { get; set; }

        public string? CreatedByPhone { get; set; }

        public string? CreatedByRole { get; set; }

        public string? AgencyName { get; set; }

        public DateTime? SubmittedOn { get; set; }

        public string? Priority { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool? AdminAssisted { get; set; }

        public int QuoteCount { get; set; }
        public int? CurrentStep { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
        public string? AssignedEmployee { get; set; }
        public string? AssignedEmployeeRole { get; set; }
    }
}
