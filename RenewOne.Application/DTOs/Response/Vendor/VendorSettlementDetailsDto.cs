namespace RenewOne.Application.DTOs.Response.Vendor
{
    public class VendorSettlementDetailsDto
    {
        public Guid VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;

        public string SettlementMethod { get; set; } = string.Empty;

        // Direct
        public decimal? AgentPaid { get; set; }
        public decimal? CommissionToReceive { get; set; }

        // Common / Admin Payment
        public decimal CompanyPayable { get; set; }

        // Commission Adjustment
        public decimal? PendingCommission { get; set; }
        public decimal? CommissionAdjusted { get; set; }
        public decimal? RemainingCommission { get; set; }
    }
}
