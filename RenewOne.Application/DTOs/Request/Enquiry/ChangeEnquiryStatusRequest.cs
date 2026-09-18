namespace RenewOne.Application.DTOs.Request.Enquiry
{
    public class ChangeEnquiryStatusRequest
    {
        public Guid EnquiryId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }
}
