namespace RenewOne.Application.DTOs.Response.Account
{
    public class AgentLookupResponse
    {
        public Guid AccountId { get; set; }

        public string AgentCode { get; set; } = string.Empty;

        public string AgentName { get; set; } = string.Empty;

        public string AgencyName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string AgentClass { get; set; } = string.Empty;

        public decimal CommissionPercentage { get; set; }

        public decimal WalletBalance { get; set; }

        public decimal LedgerBalance { get; set; }

        public bool IsActive { get; set; }
    }
}
