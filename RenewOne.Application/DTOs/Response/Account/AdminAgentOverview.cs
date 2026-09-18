namespace RenewOne.Application.DTOs.Response.Account
{
    public class AgentDashboardOverviewResponse
    {
        public BusinessSummaryDto BusinessSummary { get; set; } = new();
        public RevenueSummaryDto RevenueSummary { get; set; } = new();
        public FinancialOverviewDto FinancialOverview { get; set; } = new();
        public List<RecentEnquiryDto> RecentEnquiries { get; set; } = [];
        public List<RecentPolicyDto> RecentPolicies { get; set; } = [];
    }

    public class BusinessSummaryDto
    {
        public int TotalEnquiries { get; set; }
        public double GrowthPercentage { get; set; }
        public int PoliciesCompleted { get; set; }
        public int QuoteInProgress { get; set; }
        public int PolicyInProgress { get; set; }
    }

    public class RevenueSummaryDto
    {
        public decimal MonthlySales { get; set; }
        public decimal LifetimeSales { get; set; }
        public decimal MonthlyProfit { get; set; }
        public decimal LifetimeProfit { get; set; }
        public decimal CompanyPayable { get; set; }
        public decimal CommissionEarned { get; set; }
    }

    public class FinancialOverviewDto
    {
        public decimal WalletBalance { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal LastRechargeAmount { get; set; }
        public DateTime? LastRechargeDate { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class RecentEnquiryDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string InsuranceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Premium { get; set; }
    }

    public class RecentPolicyDto
    {
        public Guid PolicyId { get; set; }
        public string PolicyNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InsurerName { get; set; } = string.Empty;
        public decimal Premium { get; set; }
    }
}
