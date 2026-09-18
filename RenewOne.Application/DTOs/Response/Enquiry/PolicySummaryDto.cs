namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class PolicySummaryDto
    {
        public Guid EnquiryId { get; set; }

        // Customer
        public string CustomerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Address { get; set; }

        // Vehicle
        public string VehicleNumber { get; set; } = string.Empty;
        public string? Vehicle { get; set; }
        public string? VehicleType { get; set; }
        public string? FuelType { get; set; }

        // Insurance
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;

        public Guid? SelectedQuoteId { get; set; }
        // Financial
        public decimal PremiumAmount { get; set; }
        public decimal? CompanyPayable { get; set; }
        public decimal? AdminProfit { get; set; }
        public decimal AgentPayable { get; set; }
        public decimal AgentProfit { get; set; }
    }
}
