using System.ComponentModel.DataAnnotations;

namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class AddVehicleQuoteRequest
    {
        [Required]
        public Guid InsuranceCompanyId { get; set; }
        public Guid? VendorId { get; set; }
        [Required]
        public Guid EnquiryId { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Premium { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal CompanyPayable { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal AgentPoints { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal AdminProfit { get; set; }
        public bool AgentClassCommissionApplicable { get; set; }
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Idv { get; set; }
        public string Remarks { get; set; }
    }
}
