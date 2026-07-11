using System.Drawing;

namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class EnquiryListResponse
    {
        public Guid EnquiryId { get; set; }
        public string CustomerName { get; set; }
        public string ContactNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string PolicyType { get; set; }
        public string VehicleType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByName { get; set; }
    }
}
