namespace RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry
{
    public class EnquirySubmitResponse
    {
        public Guid EnquiryId { get; set; }
        public string Status { get; set; }  = string.Empty;
        public bool RedirectToQuotes { get; set; }
    }
}
