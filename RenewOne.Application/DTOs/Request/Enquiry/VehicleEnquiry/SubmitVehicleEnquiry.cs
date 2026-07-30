namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class SubmitVehicleEnquiry
    {
        public Guid EnquiryId { get; set; }
        public Guid? AccountId { get; set; }
    }
}
