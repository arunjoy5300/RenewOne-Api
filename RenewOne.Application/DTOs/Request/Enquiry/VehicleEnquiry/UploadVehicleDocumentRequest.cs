using Microsoft.AspNetCore.Http;

namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class UploadVehicleDocumentRequest
    {
        public Guid EnquiryId { get; set; }

        public string DocType { get; set; } = string.Empty;

        public string DocSide { get; set; } = string.Empty;

        public IFormFile File { get; set; } = default!;
    }
}
