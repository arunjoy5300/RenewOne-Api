namespace RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryQuoteResponse
    {
        public int VehicleEnquiryId { get; set; }
        public int QuoteId { get; set; }
        public string InsuranceCompanyName { get; set; }
        public decimal Premium { get; set; }
        public decimal IDV { get; set; }
        public decimal AgentPoints { get; set; }
    }
}
