namespace RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryResponse
    {
        public Guid EnquiryId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ManufactureYear { get; set; }
        public string? CubicCapacity { get; set; }
        public string? FuelType { get; set; }
        public string SubmissionType { get; set; }
        public int? CurrentStep { get; set; }
        public string EngineNumber { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
    }
}
