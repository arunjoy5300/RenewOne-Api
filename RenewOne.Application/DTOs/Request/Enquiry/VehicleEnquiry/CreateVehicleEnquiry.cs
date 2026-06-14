using RenewOne.Domain.Enums;

namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class CreateVehicleEnquiry
    {
        public Guid InsuranceTypeId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string? MIPolicyType { get; set; }
        public bool IsRCOwnerChanged { get; set; }
        public bool HasPreviousClaim { get; set; }
        public int NcbPercentage { get; set; }
        public int Priority { get; set; } = 1;
        public SubmissionMode SubmissionMode { get; set; }
    }
}
