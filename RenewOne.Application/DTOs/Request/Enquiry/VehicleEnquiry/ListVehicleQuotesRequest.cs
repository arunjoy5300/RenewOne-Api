namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class ListVehicleQuotesRequest
    {
        public Guid EnquiryId { get; set; }

        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public bool SortDescending { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
