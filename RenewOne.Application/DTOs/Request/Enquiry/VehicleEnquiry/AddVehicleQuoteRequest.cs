using System.ComponentModel.DataAnnotations;

namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class AddVehicleQuoteRequest
    {
        [Required]
        public Guid InsuranceCompanyId { get; set; }
        [Required]
        public Guid EnquiryId { get; set; }
        [Required]
        public decimal Premium { get; set; }
        [Required]
        public decimal CompanyPayable { get; set; }
        [Required]
        public decimal AgentPoints { get; set; }
        [Required]
        public decimal AdminProfit { get; set; }
        public decimal Idv { get; set; }
        public string Remarks { get; set; }
    }
}
